#include "workerthread.h"

WorkerThread::WorkerThread(ThreadPool *parentPool, size_t index)
    :m_isAlive(true),
	 m_pool(parentPool),
	 m_index(index),
	 m_needCheck(false),
	 m_thread(std::thread(&WorkerThread::MainLoop, this))
{
}

WorkerThread::~WorkerThread()
{
	m_isAlive = false;
	m_var.notify_all();	
    m_thread.join();
}

void WorkerThread::AddFunction(funcType type)
{
	m_func = std::move(type);
	m_needCheck = true;
	m_var.notify_one();
}

void WorkerThread::MainLoop()
{
	while (m_isAlive)
	{	
		std::unique_lock<std::mutex> locker(m_waitVarMutex);
		m_var.wait(locker, [&](){ return (m_needCheck || !m_isAlive); });
		
		if (m_needCheck)
		{
			m_needCheck = false;
			m_func();
			m_pool->Completed(m_index);
		}
	}
}
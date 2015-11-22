#include "workerthread.h"

WorkerThread::WorkerThread(ThreadPool *parent, size_t index)
    :m_isAlive(true),
	 m_parent(parent),
	 m_index(index),
	 m_isWorking(false),
	 m_needCheck(false)
{
	m_thread = std::thread(&WorkerThread::MainLoop, this);
}

WorkerThread::~WorkerThread()
{
	m_isAlive = false;
	m_var.notify_all();	
    m_thread.join();
}

bool WorkerThread::IsWorking() const
{
	std::unique_lock<std::mutex> locker(m_workingMutex);
	return m_isWorking;
}

void WorkerThread::AddFunction(funcType type)
{
	//std::unique_lock<std::mutex> locker(m_needCheckMutex);
	//std::cout << "Added function to thread " << m_index << "\n";
	m_func = std::move(type);
	m_needCheck = true;
	m_var.notify_one();
}

void WorkerThread::SetCompletedState()
{
	std::unique_lock<std::mutex> locker(m_workingMutex);
	m_isWorking = false;
}

void WorkerThread::MainLoop()
{
	while (m_isAlive)
	{	
		std::unique_lock<std::mutex> locker(m_waitVarMutex);
		m_var.wait(locker, [&](){ return (m_needCheck || !m_isAlive); });
		
		//std::unique_lock<std::mutex> checkLocker(m_needCheckMutex);
		if (m_needCheck)
		{
			m_needCheck = false;

			{
				std::unique_lock<std::mutex> locker(m_workingMutex);
				m_isWorking = true;
			}

			//std::cout << "Start executing function in thread " << m_index << std::endl;
			m_func();
			//std::cout << "Ended executing function in thread " << m_index << std::endl;
			m_parent->Completed(m_index);

			//std::cout << "End of thread work\n";
		}
	}
}
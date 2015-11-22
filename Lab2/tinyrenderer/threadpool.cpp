#include "threadpool.h"
#include "workerthread.h"
#include <chrono>
#include <iostream>
#include <fstream>

ThreadPool::ThreadPool(size_t threadCount)
	:m_tasks(), m_runningTasks(), m_workingThreads(), 
	m_isAlive(true), m_notified(false), m_waitCheck(false), m_needWait(false), fout("log.txt")
{
    for (size_t i = 0; i < threadCount; ++i)
    {
		m_threads.push_back(thread_ptr(new WorkerThread(this, i)));
		m_workingThreads.push_back(false);
    }

	m_thread = std::thread(&ThreadPool::MainLoop, this);
}

ThreadPool::~ThreadPool()
{
	m_isAlive = false;
	m_cv.notify_all();
	m_thread.join();
}

void ThreadPool::Completed(size_t threadIndex)
{
	{
		std::unique_lock<std::mutex> locker(m_runningTasksMutex);
		m_runningTasks.erase(threadIndex);
	}
	
	{
		std::unique_lock<std::mutex> locker(m_workingMutex);
		m_workingThreads[threadIndex] = false;
	}

	m_notified = true;
	m_cv.notify_one();

	if (m_needWait)
	{
		m_waitCheck = true;
		m_waitVar.notify_one();
	}
}

void ThreadPool::WaitForTasks() const
{
	std::unique_lock<std::mutex> lockerRunning(m_endFrameWaiting);
	m_needWait = true;
	
	while (!m_runningTasks.empty() || !m_tasks.empty())
	{
		m_waitVar.wait(lockerRunning, [&](){ return m_waitCheck; });
		m_waitCheck = false;
	}

	m_needWait = false;
}

void ThreadPool::MainLoop()
{
	std::unique_lock<std::mutex> locker(m_waitingMutex);
	while (m_isAlive)
	{	
		m_cv.wait(locker, [&](){ return ((m_notified && !m_tasks.empty()) || !m_isAlive); });

		if (m_notified && !m_tasks.empty())
		{
			m_notified = false;

			for (size_t i = 0; i < m_threads.size() && !m_tasks.empty(); ++i)
			{
				if (m_threads[i] && !m_workingThreads[i])
				{
					funcType task;
					{
						std::unique_lock<std::mutex> lockerTask(m_taskMutex);
						task = m_tasks.front();
						m_tasks.pop();
					}

					{
						std::unique_lock<std::mutex> lockerRunning(m_runningTasksMutex);
						m_runningTasks.insert(i);
					}

					m_workingThreads[i] = true;
					m_threads[i]->AddFunction(task);
				}
			}
		}
	}
}
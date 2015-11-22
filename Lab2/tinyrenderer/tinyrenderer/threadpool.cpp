#include "threadpool.h"

ThreadPool::ThreadPool(size_t threadCount)
	:m_tasks(), m_runningTasks(), m_isAlive(true), m_notified(false), m_waitCheck(false), m_needWait(false)
{
    for (size_t i = 0; i < threadCount; ++i)
    {
		m_threads.push_back(thread_ptr(new WorkerThread(this, i)));
    }

	m_thread = std::thread(&ThreadPool::MainLoop, this);
}

ThreadPool::~ThreadPool()
{
	m_isAlive = false;

	{
		std::unique_lock<std::mutex> locker(m_notifyMutex);
		m_notified = true;
		m_cv.notify_all();
	}

	m_thread.join();
}

void ThreadPool::Completed(size_t threadIndex)
{
	{
		std::unique_lock<std::mutex> locker(m_runningTasksMutex);
		m_runningTasks.erase(threadIndex);
	}
	
	m_threads[threadIndex]->SetCompletedState();
	std::cout << "Thread " << threadIndex << "ended work\n";
	{
		std::unique_lock<std::mutex> locker(m_notifyMutex);
		m_notified = true;
		m_cv.notify_one();
	}

	{
		std::unique_lock<std::mutex> lockerRunning(m_needWaitMutex);
		if (m_needWait)
		{
			//std::cout << "Notifying pool for end\n";
			m_waitCheck = true;
			m_waitVar.notify_one();
		}
	}
}

void ThreadPool::WaitForTasks() const
{
	//std::cout << "Start waiting for frame end\n";
	{
		std::unique_lock<std::mutex> lockerRunning(m_needWaitMutex);
		m_needWait = true;
	}

	{
		std::unique_lock<std::mutex> lockerRunning(m_waitableAgain);
		while (!m_runningTasks.empty() || !m_tasks.empty())
		{
			m_waitVar.wait(lockerRunning, [&](){ return m_waitCheck; });
			m_waitCheck = false;
			//std::cout << "Got frame\n";
		}
	}

	{
		std::unique_lock<std::mutex> lockerRunning(m_needWaitMutex);
		m_needWait = false;
	}
}

void ThreadPool::MainLoop()
{
	while (m_isAlive)
	{
		{
			std::unique_lock<std::mutex> locker(m_waitingMutex);
			m_cv.wait(locker, [&](){ return ((m_notified && !m_tasks.empty()) || !m_isAlive); });
		}

		{
			//std::unique_lock<std::mutex> lockerTask(m_taskMutex);
			if (!m_tasks.empty())
			{
				for (size_t i = 0; i < m_threads.size() && !m_tasks.empty(); ++i)
				{
					if (m_threads[i] && !m_threads[i]->IsWorking())
					{
						auto task = m_tasks.front();
						m_tasks.pop();

						{
							std::unique_lock<std::mutex> lockerRunning(m_runningTasksMutex);
							m_runningTasks.insert(i);
						}

						m_threads[i]->AddFunction(task);
						//std::cout << "Added to pool in thread " << i << "\n";
						//break;
					}
				}
			}
		}
		
		{
			std::unique_lock<std::mutex> locker(m_notifyMutex);
			m_notified = false;
		}
	}
}

//bool ThreadPool::IsWorking()
//{
//	for (size_t i = 0; i < m_threads.size(); ++i)
//	{
//
//	}
//}
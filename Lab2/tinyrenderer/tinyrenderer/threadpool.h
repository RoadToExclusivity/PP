#ifndef THREADPOOL_H
#define THREADPOOL_H

#include "workerthread.h"
#include <vector>
#include <memory>
#include <queue>
#include <mutex>
#include <thread>
#include <condition_variable>
#include <set>
#include "threads_include.h"

class WorkerThread;
typedef std::shared_ptr<WorkerThread> thread_ptr;

class ThreadPool
{
public:
    ThreadPool(size_t threadCount);
	~ThreadPool();

    template<class FuncName>
    void RunAsync(FuncName func, Model &model, FrameTile &tile)
    {
		std::unique_lock<std::mutex> locker(m_notifyMutex);
		{
			std::unique_lock<std::mutex> locker(m_taskMutex);
			m_tasks.push(std::bind(func, std::ref(model), std::ref(tile)));
			//std::cout << "Added to pool\n";
		}
		
		m_notified = true;
		m_cv.notify_one();
    }

	void Completed(size_t threadIndex);
	void WaitForTasks() const;
	//bool IsWorking();

private:
	void MainLoop();
	std::queue<funcType> m_tasks;
	std::set<size_t> m_runningTasks;
    std::vector<thread_ptr> m_threads;

	mutable std::mutex m_taskMutex, m_runningTasksMutex, m_waitingMutex, m_notifyMutex, m_mainLoopMutex, m_needWaitMutex, m_waitableAgain;

	bool m_isAlive, m_notified;
	mutable bool m_waitCheck, m_needWait;
	std::condition_variable m_cv;
	mutable std::condition_variable m_waitVar;
	std::thread m_thread;
};

#endif // THREADPOOL_H

#ifndef WORKERTHREAD_H
#define WORKERTHREAD_H

#include <thread>
#include <mutex>
#include <condition_variable>
#include "threadpool.h"
#include "threads_include.h"

class ThreadPool;

class WorkerThread
{
public:
	WorkerThread(ThreadPool* parentPool, size_t index);
    ~WorkerThread();

    void AddFunction(funcType type);

private:
    void MainLoop();

	mutable std::mutex m_waitVarMutex;
    std::condition_variable m_var;

	bool m_isAlive, m_needCheck;
	ThreadPool *m_pool;
	size_t m_index;
	funcType m_func;

	std::thread m_thread;
};

#endif // WORKERTHREAD_H

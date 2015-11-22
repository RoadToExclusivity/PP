#ifndef WORKERTHREAD_H
#define WORKERTHREAD_H

#include <thread>
#include <mutex>
#include <functional>
#include <queue>
#include "frametile.h"
#include "model.h"
#include <condition_variable>
#include "threadpool.h"
#include "threads_include.h"

class ThreadPool;

class WorkerThread
{
	friend class ThreadPool;

public:
	WorkerThread(ThreadPool* parent, size_t index);
    ~WorkerThread();

    void AddFunction(funcType type);
	bool IsWorking() const;

private:
    void MainLoop();
	void SetCompletedState();

    mutable std::mutex m_mutex;
	mutable std::mutex m_workingMutex;
	mutable std::mutex m_waitVarMutex;
    std::condition_variable m_var;

	bool m_isAlive, m_needCheck;
	ThreadPool *m_parent;
	size_t m_index;
	funcType m_func;
	bool m_isWorking;

	std::thread m_thread;
};

#endif // WORKERTHREAD_H

from multiprocessing import JoinableQueue, Process, Manager
import time
import random

SQUARE_SIDE = 1000000
SQR_CIRCLE_RADIUS = SQUARE_SIDE * SQUARE_SIDE
ONE_ITERATION_MINIMUM_SIZE = 200000


def calc_pi(iterations):
    random.seed()

    hits = 0
    while iterations > 0:
        x = random.randint(0, SQUARE_SIDE)
        y = random.randint(0, SQUARE_SIDE)
        if x ** 2 + y ** 2 <= SQR_CIRCLE_RADIUS:
            hits += 1

        iterations -= 1

    return hits


def get_iterations_partition(iterations, process_count):
    partition = []

    while iterations > 0:
        if ONE_ITERATION_MINIMUM_SIZE * process_count <= iterations:
            for i in range(process_count):
                partition.append(ONE_ITERATION_MINIMUM_SIZE)
            iterations -= ONE_ITERATION_MINIMUM_SIZE * process_count
        else:
            for i in range(process_count):
                cur_partition = iterations // (process_count - i)
                iterations -= cur_partition
                partition.append(cur_partition)

    return partition


def worker(q, return_list):
    cur_result = 0
    while True:
        cur_iteration = q.get()
        if cur_iteration is None:
            break

        cur_ans = calc_pi(cur_iteration)
        cur_result += cur_ans

        q.task_done()
    q.task_done()
    return_list.append(cur_result)


def print_results(time, result_list, iterations):
    calculated_result = 0
    for res in result_list:
        calculated_result += res

    print("Time {0:.3f}".format(time))
    print("%.10f" % (calculated_result * 4 / iterations))


def solve(iterations, proc_count):

    queue = JoinableQueue()
    partition = get_iterations_partition(iterations, proc_count)
    for iteration in partition:
        queue.put(iteration)
    for i in range(process_count):
        queue.put(None)

    manager = Manager()
    result = manager.list()
    processes = []

    cur_time = time.time()
    for i in range(process_count):
        proc = Process(target=worker, args=(queue, result,))
        proc.start()
        processes.append(proc)

    queue.join()
    for proc in processes:
        proc.join()

    cur_time = time.time() - cur_time
    print_results(cur_time, result, iterations)

if __name__ == '__main__':

    iteration_count = int(input('Iterations count : '))
    process_count = int(input('Process count : '))

    solve(iteration_count, process_count)

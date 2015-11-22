# For single run : 78.44

from multiprocessing import Pool
import time
import random

SQUARE_SIDE = 1000000
SQR_CIRCLE_RADIUS = SQUARE_SIDE * SQUARE_SIDE


def calc_pi(iterations):
    random.seed()

    result = 0
    while iterations > 0:
        x = random.randint(0, SQUARE_SIDE)
        y = random.randint(0, SQUARE_SIDE)
        if x ** 2 + y ** 2 <= SQR_CIRCLE_RADIUS:
            result += 1

        iterations -= 1

    return result


def get_iterations_partition(iterations, processes):
    partition = []

    for i in range(processes):
        cur_partition = iterations / (processes - i)
        iterations -= cur_partition
        partition.append(cur_partition)

    return partition

if __name__ == '__main__':

    iterationCount = int(input('Iterations count : '))
    processCount = int(input('Process count : '))

    pool = Pool(processes=processCount)

    curTime = time.time()
    result = pool.map(calc_pi, get_iterations_partition(iterationCount, processCount))
    curTime = time.time() - curTime

    pool.close()
    pool.join()

    calculated_result = 0
    for res in result:
        calculated_result += res

    print("Time {0:.3f}".format(curTime))
    print("%.10f" % (calculated_result * 4 / iterationCount))

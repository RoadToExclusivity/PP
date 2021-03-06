import threading

ALPHABET_LETTERS = 26
LETTERS_IN_LINE = 5000
LINES_PER_LETTER = 100
LINES_COUNT = LINES_PER_LETTER * ALPHABET_LETTERS
MAX_THREADS = LINES_COUNT

lock = threading.Lock()
event_lock = threading.Lock()
events = []
depending_events = []
semaphore = threading.Semaphore()

isLock = False
isEvent = False
isSemaphore = False

rest_lines_count = []


def set_synch_type(synch_type):
    global isLock
    global isEvent
    global isSemaphore

    isLock = synch_type == 1
    isEvent = synch_type == 2
    isSemaphore = synch_type == 3


def print_letters(letter):
    for j in range(LETTERS_IN_LINE):
        print(letter, end="")
    print()


def start_synchronize(thread_number):
    if isLock:
        lock.acquire()
    elif isEvent:
        events[thread_number].wait()
    elif isSemaphore:
        semaphore.acquire()


def stop_synchronize(thread_number, thread_count):
    if isLock:
        lock.release()
    elif isEvent:
        events[thread_number].clear()
        events[(thread_number + 1) % thread_count].set()
    elif isSemaphore:
        semaphore.release()


def thread_loop(thread_number, thread_count):
    global rest_lines_count
    was_print = True
    while was_print:
        was_print = False
        for i in range(ALPHABET_LETTERS):
            need_print = False

            start_synchronize(thread_number)

            if rest_lines_count[i] > 0:
                rest_lines_count[i] -= 1
                was_print = True
                need_print = True

            if need_print:
                print_letters(chr(ord('a') + i))

            stop_synchronize(thread_number, thread_count)


def launch_threads(thread_count):
    for i in range(ALPHABET_LETTERS):
        rest_lines_count.append(LINES_PER_LETTER)

    if isEvent:
        for i in range(thread_count + 1):
            events.append(threading.Event())
        events[0].set()

    threads = []
    for i in range(thread_count):
        threads.append(threading.Thread(target=thread_loop, args=(i, thread_count,)))
        threads[i].start()

    for i in range(thread_count):
        threads[i].join()


if __name__ == '__main__':
    synch_type = int(input('Thread synchronize type (0 - none, 1 - lock, 2 - event, 3 - semaphore) -> '))
    if synch_type > 3 or synch_type < 0:
        print('Wrong synch type')
    else:
        set_synch_type(synch_type)

        thread_count = int(input('Thread count -> '))
        if thread_count < 1:
            thread_count = 1
        if thread_count > MAX_THREADS:
            thread_count = MAX_THREADS
        
        launch_threads(thread_count)

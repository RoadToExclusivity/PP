import string
import threading

LETTERS_COUNT = 26
LETTER_COUNT_PER_LINE = 5000
LINES_PER_LETTER = 100
LINES_COUNT = LINES_PER_LETTER * LETTERS_COUNT
MAX_THREADS = LINES_COUNT

lock = threading.Lock()
events = []
semaphore = threading.Semaphore()

def set_synch_vars():
    global isLock
    global isEvent
    global isSemaphore
    
    isLock = synchType == 1
    isEvent = synchType == 2
    isSemaphore = synchType == 3

def print_letters(letter):
    for j in range(LETTER_COUNT_PER_LINE):
        print(letter, end="")

def print_lines(lineStart, lineEnd, threadNumber):
    if isLock:
        lock.acquire()
    if isEvent:
        events[threadNumber].wait()
    if isSemaphore:
        semaphore.acquire()

    #print('---Thread %d has started' % threadNumber)
    
    for i in range(lineStart, lineEnd):
        letter = chr(ord('a') + (i // LINES_PER_LETTER))
        print_letters(letter)
        print()

    #print('---Thread %d has ended' % threadNumber)
    if isLock:
        lock.release()
    if isEvent:
        events[(threadNumber + 1) % threadCount].set()
    if isSemaphore:
        semaphore.release()

def init_threads(threadCount):
    global threads
    threads = []
    lineStart = 0

    if isEvent:
        for i in range(threadCount):
            events.append(threading.Event())
        events[0].set()
        
    for i in range(threadCount):
        lineCount = (LINES_COUNT - lineStart) // (threadCount - i)
        threads.append(threading.Thread(target = print_lines, args = (lineStart, lineStart + lineCount, i)))
        threads[i].start()
        lineStart += lineCount

    for i in range(threadCount):
        threads[i].join()
        
if __name__ == "__main__":
    global synchType
    synchType = int(input('Thread synchronize type (0 - none, 1 - lock, 2 - event, 3 - semaphore) -> '))
    if synchType > 3 or synchType < 0:
        print('Wrong synch type')
    else:
        threadCount = int(input('Thread count -> '))
        set_synch_vars()
        if threadCount < 1:
            threadCount = 1
        if threadCount > MAX_THREADS:
            threadCount = MAX_THREADS
        
        init_threads(threadCount)

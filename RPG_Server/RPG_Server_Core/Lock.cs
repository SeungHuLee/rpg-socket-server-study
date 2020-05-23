using System.Threading;

namespace RPG_Server_Core
{
    // 재귀적 lock을 허용할 것인가? (YES) 
    // - WriteLock 상태에서 또 WriteLock잡기 OK
    // - WriteLock -> ReadLock OK
    // - ReadLock -> WriteLock NO

    // : WriteLock을 Acquire한 상태에서 또다시 한번 같은 스레드에서 Acquire 시도를 허용할 지
    // 스핀락 정책(5000번 -> Yield)
    class Lock
    {
        const int EMPTY_FLAG = 0x00000000;
        // 맨 앞 1개 사용하면 -2,147,483,648 음수가 나올 수 있기 때문에 사용 안함
        const int WRITE_MASK = 0x7FFF0000; // Unused 하나, WriteThreadID 15개
        const int READ_MASK = 0x0000FFFF;
        const int MAX_SPIN_COUNT = 5000;

        // int형 32비트
        // [Unused(1)] [WriteThreadID(15)] [ReadCount(16)]
        // WriteThreadID : Write는 한번에 한 쓰레드만 가능하니 그게 누구인지 기록
        // ReadCount? : 여러 스레드가 Read를 잡으려 할 수 있기 때문
        int _flag = EMPTY_FLAG;
        int _writeCount = 0;

        public void WriteLock()
        {
            // 동일 스레드가 WriteLock을 이미 획득하고 있는 지 확인 (재귀용)
            int lockThreadId = (_flag & WRITE_MASK) >> 16;
            if (Thread.CurrentThread.ManagedThreadId == lockThreadId)
            {
                _writeCount++;
                return;
            }

            // 아무도 WriteLock/ReadLock을 획득하고 있지 않을 때, 경합해서 소유권을 얻는다.
            int desired =  (Thread.CurrentThread.ManagedThreadId << 16) & WRITE_MASK;
            while (true)
            {
                for (int i = 0; i < MAX_SPIN_COUNT; i++)
                {
                    if (Interlocked.CompareExchange(ref _flag, desired, EMPTY_FLAG) == EMPTY_FLAG)
                    {
                        _writeCount = 1;
                        return;
                    }
                    // 시도를 해서 성공하면 return
                }
                Thread.Yield();
            }
        }

        public void WriteUnlock()
        {
            int lockCount = --_writeCount;
            if (lockCount == 0) { Interlocked.Exchange(ref _flag, EMPTY_FLAG); }
        }

        public void ReadLock()
        {
            // 동일 스레드가 WriteLock을 이미 획득하고 있는 지 확인 (재귀용)
            int lockThreadId = (_flag & WRITE_MASK) >> 16;
            if (Thread.CurrentThread.ManagedThreadId == lockThreadId)
            {
                Interlocked.Increment(ref _flag);
                return;
            }

            // 아무도 WriteLock을 획득하고 있지 않으면 ReadCount를 1 늘린다.
            while (true)
            {
                for (int i = 0; i < MAX_SPIN_COUNT; i++)
                {
                    int expected = (_flag & READ_MASK);
                    if (Interlocked.CompareExchange(ref _flag, expected + 1, expected) == expected)
                    {
                        return;
                    }
                }
                Thread.Yield();
            }
        }

        public void ReadUnlock()
        {
            Interlocked.Decrement(ref _flag);
        }
    }
}
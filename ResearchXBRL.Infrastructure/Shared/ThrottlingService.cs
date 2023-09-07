using System;
using System.Collections.Generic;
using System.Linq;

namespace ResearchXBRL.Infrastructure.Shared;

sealed class ThrottlingService
{
    private readonly int baseValue;
    private IEnumerator<int> level = Feb().GetEnumerator();
    private int slowDownCount = 0;
    public int HealingTime => level.Current * baseValue;

    public ThrottlingService(in int baseValue)
    {
        this.baseValue = baseValue;
        level.MoveNext();
    }

    public void SlowDown()
    {
        lock (this)
        {
            slowDownCount++;
            level.MoveNext();
        }
    }


    public void Reset()
    {
        lock (this)
        {
            if (slowDownCount > 1)
            {
                slowDownCount--;
                return;
            }

            level = Feb().GetEnumerator();
            level.MoveNext();
            slowDownCount = 0;
        }
    }

    private static IEnumerable<int> Feb()
    {
        var array = new int[] { 0, 1 };
        while (true)
        {
            var fibo = array[0] + array[1];
            array[0] = array[1];
            array[1] = fibo;
            yield return fibo;
        }
    }
}
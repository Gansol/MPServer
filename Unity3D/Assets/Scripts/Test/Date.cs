using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Data;
using System;

public class Date : MonoBehaviour
{
    public int thisYear;
    public int count;

    // Start is called before the first frame update
    void Start()
    {
        int startYear, endYear = thisYear + count, x = -1, y = -2; ;

        for (startYear = thisYear - count; startYear < endYear; startYear++)
        {
            DateTime dtThisYear = new DateTime(thisYear, 1, 1);
            DateTime dtYear = new DateTime(startYear, 1, 1);

            if (dtYear.DayOfWeek == dtThisYear.DayOfWeek) x = startYear;

            int dtOldDays = System.DateTime.DaysInMonth(startYear, 2);
            int dtThisDays = System.DateTime.DaysInMonth(thisYear, 2);

            if (dtOldDays == dtThisDays) y = startYear;

            if (x == y)
            {
                x = -1;
                y = -2;
                Debug.Log(" Same Year: " + startYear);
            }
        }
    }
}

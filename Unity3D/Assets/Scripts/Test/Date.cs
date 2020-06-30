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
        int startYear;
        int endYear = thisYear + count;

        DateTime dtThisYear, dtYear;
        int dtOldDays, dtThisDays, x = -1, y = -2;

        for (startYear = (thisYear - count); startYear < endYear; startYear++)
        {
            dtThisYear = new DateTime(thisYear, 1, 1);
            dtYear = new DateTime(startYear, 1, 1);

            if (dtYear.DayOfWeek == dtThisYear.DayOfWeek) x = startYear;


            dtOldDays = System.DateTime.DaysInMonth(startYear, 2);
            dtThisDays = System.DateTime.DaysInMonth(thisYear, 2);

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

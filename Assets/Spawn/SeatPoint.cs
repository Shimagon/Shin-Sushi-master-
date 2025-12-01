using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SeatPoint : MonoBehaviour
{
    [Header("椅子番号（1〜6など）")]
    public int seatId;

    [Header("この席が使用中かどうか")]
    public bool isOccupied = false;
}

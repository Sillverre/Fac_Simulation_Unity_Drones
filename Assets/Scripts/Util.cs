using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Util {

	private static float cleanDelay = 45;
    public static List<Waste> cleanWasteList(List<Waste> wasteList)
	{
			for (int i = wasteList.Count - 1; i >= 0; i--)
			{
					if (wasteList[i].Pickedup && Time.time - wasteList[i].At > cleanDelay)
					{
							wasteList.RemoveAt(i);
					}
			}
        return wasteList;
	}

    private static float epsilon = 2;
    public static Waste findWasteInList(Waste waste, List<Waste> wasteList)
    {
        float x_min = waste.Position.x - epsilon;
        float x_max = waste.Position.x + epsilon;
        float z_min = waste.Position.z - epsilon;
        float z_max = waste.Position.z + epsilon;
        foreach (Waste w in wasteList)
        {
            if (waste.Tag == w.Tag && x_min <= w.Position.x && x_max >= w.Position.x && z_min <= w.Position.z && z_max >= w.Position.z)
            {
                return w;
            }
        }
        return null;
    }
}

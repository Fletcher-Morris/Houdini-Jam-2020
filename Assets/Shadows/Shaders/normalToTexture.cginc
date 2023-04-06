void Normal_To_Texture_float(float3 Normal, out float Out_Index)
{
	Out_Index = 0;

	float max = 0;

	//	Forward/Backward
	float z = Normal.z;
	if (z > max)
	{
		max = z;
		Out_Index = 0;
	}
	else if (-z > max)
	{
		max = -z;
		Out_Index = 1;
	}

	//	Right/Left
	float x = Normal.x;
	if (x > max)
	{
		max = x;
		Out_Index = 2;
	}
	else if (-x > max)
	{
		max = -x;
		Out_Index = 3;
	}

	//	Up/Down
	float y = Normal.y;
	if (y > max)
	{
		max = y;
		Out_Index = 4;
	}
	else if (-y > max)
	{
		max = -y;
		Out_Index = 5;
	}
}
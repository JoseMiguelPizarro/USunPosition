using System;
using UnityEngine;
using static UnityEngine.Mathf;

[Serializable]
public struct SphericalCoordinates
{
	public float azimuth;
	public float zenith;
	public float radius;

	public SphericalCoordinates(float azimuth, float zenith) : this(azimuth, zenith, 1.0f) { }

	public SphericalCoordinates(float azimuth, float zenith, float radius)
	{
		this.azimuth = azimuth;
		this.zenith = zenith;
		this.radius = radius;
	}

	public Vector3 ToCartesianCoordinates()
	{
		return new Vector3(Sin(zenith) * Sin(azimuth), Sin(zenith) * Cos(azimuth), Cos(zenith)) * radius;
	}
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Globalization;
using Sirenix.OdinInspector;
using static UnityEngine.Mathf;

[ExecuteAlways]
public partial class PSAStar : MonoBehaviour
{
	private const float P0 = 2.267127827f;
	private const float P1 = -9.300339267e-4f;
	private const float P2 = 4.895036035f;
	private const float P3 = 1.720279602e-2f;
	private const float P4 = 6.239468336f;
	private const float P5 = 1.720200135e-2f;

	private const float P6 = 3.338320972e-2f;
	private const float P7 = 3.497596876e-4f;
	private const float P8 = -1.544353226e-4f;
	private const float P9 = -8.689729360e-6f;

	private const float P10 = 4.090904909e-1f;
	private const float P11 = -6.213605399e-9f;
	private const float P12 = 4.418094944e-5F;

	private const float P13 = 6.697096103f;
	private const float P14 = 6.570984737e-2f;

	public Transform directionalLight;
	public Transform sun;

	private const float EarthMediumRadius = 6371.01f;
	private const float AtronomicalUnit = 149597890.0f;


	public int Year;
	public int Month;
	public int Day;

	public float hour;

	public float latitude;
	public float longitude;

	public SphereCoordinates coordinates;

	private Vector3 direction;

	private void Update()
	{
		coordinates = PSA(Year, Month, Day, hour, latitude, longitude);
		direction = -new Vector3(Sin(coordinates.zenith) * Sin(coordinates.azimuth), Sin(coordinates.zenith) * Cos(coordinates.azimuth), Cos(coordinates.zenith));

		var left = Vector3.Cross(direction, Vector3.up);
		var up = Vector3.Cross(direction, left);

		var rotation = Quaternion.LookRotation(direction, up);

		directionalLight.transform.rotation = rotation;
	}

	private void OnDrawGizmos()
	{
		Gizmos.color = Color.green;
		Gizmos.DrawLine(transform.position, transform.position + direction * 5);
	}


	[ReadOnly]
	public float julianDate;

	/// <summary>
	/// Converts from gregorian to julian
	/// </summary>
	/// <see cref="https://en.wikipedia.org/wiki/Julian_day#Converting_Gregorian_calendar_date_to_Julian_Day_Number"/>
	/// <param name="Y"></param>
	/// <param name="M"></param>
	/// <param name="D"></param>
	public int GregorianToJulian(int Y, int M, int D)
	{
		var julian = (1461 * (Y + 4800 + (M - 14) / 12)) / 4 + (367 * (M - 2 - 12 * ((M - 14) / 12))) / 12 - (3 * ((Y + 4900 + (M - 14) / 12) / 100)) / 4 + D - 32075;
		return julian;
	}

	/// <summary>
	/// Based on 
	/// </summary>
	/// <see cref="https://doi.org/10.1016/S0038-092X(00)00156-0"/>
	/// <param name="year"></param>
	/// <param name="mont"></param>
	/// <param name="day"></param>
	/// <param name="hour"></param>
	/// <returns></returns>
	public SphereCoordinates PSA(int year, int mont, int day, float hour, float longitude, float latitude)
	{
		var julianDate = GetJulianDate(year, mont, day) + hour / 24.0f;

		var n = julianDate - 2451545.0f;
		var omega = 2.1429f - 0.0010394594f * n;
		var meanLongitude = 4.8950630f + 0.017202791698f * n;
		var meanAnomaly = 6.2400600f + 0.0172019699f * n;

		var eclipticLongitude = meanLongitude + 0.03341607f * Sin(meanAnomaly) + 0.00034894f * Sin(2 * meanAnomaly) - 0.0001134f - 0.0000203f * Sin(omega);
		var eclipticObliquity = 0.4090928f - 6.2140e-9f * n + 0.0000396f * Cos(omega);

		var rightAscention = Atan2(Cos(eclipticObliquity) * Sin(eclipticLongitude), Cos(eclipticLongitude));

		var declination = Asin(Sin(eclipticObliquity) * Sin(eclipticLongitude));

		if (rightAscention < 0.0f)
			rightAscention += 2 * PI;

		var gmst = 6.6974243242f + 0.0657098283f * n + hour;

		var lmst = (gmst * 15 + longitude) * (PI / 180);
		var hourAngle = lmst - rightAscention;

		var zenit = Acos(Cos(latitude) * Cos(hourAngle) * Cos(declination) + Sin(declination) * Sin(latitude));
		var azimut = Atan2(-Sin(hourAngle), (Tan(declination) * Cos(latitude) - Sin(latitude) * Cos(hourAngle)));

		var parallax = EarthMediumRadius / AtronomicalUnit * Sin(zenit);

		zenit = zenit + parallax;

		return new SphereCoordinates(azimut, zenit);
	}

	public SphereCoordinates GetSunPosition(int year, int month, int day, float hour, float minute, float second, float latitude, float longitude)
	{
		float julianDate = GetJulianDate(year, month, day);

		var ellapsedJulianDays = julianDate - 2451545.0f;

		var omega = P0 + P1 * ellapsedJulianDays;
		var L = P2 + P3 * ellapsedJulianDays;
		var g = P4 + P5 * ellapsedJulianDays;

		var eclipticLongitude = L + P6 * Sin(g) + P7 * Sin(2 * g) + P8 + P9 * Sin(omega);
		var eclipticObliquity = P10 + P11 * ellapsedJulianDays + P12 * Cos(omega);

		var rightAscention = Atan(Cos(eclipticObliquity) * Sin(eclipticLongitude) / Cos(eclipticLongitude));
		var declination = Asin(Sin(eclipticObliquity) * Sin(eclipticLongitude));

		if (rightAscention < 0.0f)
			rightAscention += 2 * PI;

		var gmst = P13 + P14 * ellapsedJulianDays + (hour + minute / 60f + second / 3600f);

		var w = (gmst * PI / 12.0f + longitude);
		var azimut = Atan(-Sin(w) / (Tan(declination) * Cos(latitude) - Sin(latitude) * Cos(w)));

		var zenith = Acos(Cos(latitude) * Cos(declination) * Cos(w) + Sin(latitude) * Sin(declination));

		var earthMediumRadius = 6371.01f;
		var astronomicalUnit = 149597870.7f;

		zenith = zenith + earthMediumRadius / (astronomicalUnit) * Sin(zenith);

		if (azimut > 0)
		{
			azimut -= PI * 2;
		}


		return new SphereCoordinates(azimut, zenith);
	}

	private float GetJulianDate(int year, int month, int day)
	{
		var julianDay = GregorianToJulian(year, month, day);
		//var julianHour = (hour - 12) / 24.0f + minute / 1440.0f + second / 86400.0f;

		//if (hour < 12.0f)
		//	julianHour += 1.0f;

		var julianDate = (float)julianDay;
		return julianDate;
	}
}

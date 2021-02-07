using UnityEngine;
using static UnityEngine.Mathf;

public static class SunPosition
{
	private const float PARALLAX_FACTOR = 4.258756e-5f;

	public static SphericalCoordinates AlignToLightDirection(Transform transform, int year, int month, int day, float hour, float longitude, float latitude)
	{
		var coordinates = GetSunCoordinates(year, month, day, hour, longitude, latitude);

		transform.rotation = coordinates.GetFlippedRotation();

		return coordinates;
	}
	

	/// <summary>
	/// Computes the sun position using the PSA algorithm
	/// </summary>
	/// <see cref="https://www.sciencedirect.com/science/article/abs/pii/S0038092X00001560"/>
	/// <param name="hour">Time in hours</param>
	/// <param name="longitude">Longitude in degrees</param>
	/// <param name="latitude">Latitude in degrees</param>
	/// <returns>Sun position in spherical coordinates</returns>
	public static SphericalCoordinates GetSunCoordinates(int year, int month, int day, float hour, float longitude, float latitude)
	{
		var julianDate = GetJulianDate(year, month, day) + hour / 24.0f;

		var n = julianDate - 2451545.0f;
		var omega = 2.1429f - 0.0010394594f * n;
		var meanLongitude = 4.8950630f + 0.017202791698f * n;
		var meanAnomaly = 6.2400600f + 0.0172019699f * n;

		var eclipticLongitude = meanLongitude + 0.03341607f * Sin(meanAnomaly) + 0.00034894f * Sin(2 * meanAnomaly) - 0.0001134f - 0.0000203f * Sin(omega);
		var eclipticObliquity = 0.4090928f - 6.2140e-9f * n + 0.0000396f * Cos(omega);

		var sinEclipticLongitude = Sin(eclipticLongitude);

		var rightAscention = Atan2(Cos(eclipticObliquity) * sinEclipticLongitude, Cos(eclipticLongitude));

		var declination = Asin(Sin(eclipticObliquity) * sinEclipticLongitude);

		if (rightAscention < 0.0f)
			rightAscention += 2 * PI;

		var gmst = 6.6974243242f + 0.0657098283f * n + hour;

		var lmst = (gmst * 15 + longitude) * (PI / 180);
		var hourAngle = lmst - rightAscention;
		var cosLatitude = Cos(latitude);
		var sinLatitude = Sin(latitude);
		var cosHourAngle = Cos(hourAngle);
		

		var zenit = Acos(cosLatitude * cosHourAngle * Cos(declination) + Sin(declination) * sinLatitude);
		var azimut = Atan2(-Sin(hourAngle), (Tan(declination) * cosLatitude - sinLatitude * cosHourAngle));

		var parallax = PARALLAX_FACTOR * Sin(zenit);

		zenit = zenit + parallax;

		return new SphericalCoordinates(azimut, zenit, 1.0f);
	}

	public static float GetJulianDate(int year, int month, int day)
	{
		return (1461 * (year + 4800 + (month - 14) / 12)) / 4 + (367 * (month - 2 - 12 * ((month - 14) / 12))) / 12 - (3 * ((year + 4900 + (month - 14) / 12) / 100)) / 4 + day - 32075;
	}
}
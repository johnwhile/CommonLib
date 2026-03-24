#include "pch.h"
#include "Common.Clr.h"
#include "cmath"

using namespace CommonClr;

Float3 CommonClr::MathCpp::CartesianToSpherical(Float3 cartesian)
{
	float r = cartesian.Normalize();
	float theta = atan2(cartesian.z, cartesian.x);
	float phi = acos(cartesian.y);
	return Float3(r, theta, phi);
}
Float3 CommonClr::MathCpp::SphericalToCartesian(Float3 spherical)
{
	float sinphi = sin(spherical.z);	
	return Float3(
		spherical.x * cos(spherical.y) * sinphi,
		spherical.x * cos(spherical.z),
		spherical.x * sin(spherical.y) * sinphi);
}

CommonClr::Float3::Float3(float x, float y, float z)
{
	this->x = x;
	this->y = y;
	this->z = z;
}

float CommonClr::Float3::Normalize()
{
	float length = sqrtf(x * x + y * y + z * z);
	float l = length > 1e-6f ? 1.f/length : 0;
	x *= l;
	y *= l;
	z *= l;
	return length;
}
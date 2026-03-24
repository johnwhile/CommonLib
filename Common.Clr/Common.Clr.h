#pragma once

using namespace System;

namespace CommonClr 
{
	//"value struct" is equivalent to C# struct
	public value struct Float3
	{
	public:
		Float3(float x, float y, float z);
		float x, y, z;
		float Normalize();
	};


	public ref class MathCpp
	{
		// TODO: Aggiungere qui i metodi per questa classe.

	public :

		static Float3 CartesianToSpherical(Float3 cartesian);
		static Float3 SphericalToCartesian(Float3 spherical);

	};






}

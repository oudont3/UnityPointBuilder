// https://gist.github.com/mattatz/40a91588d5fb38240403f198a938a593
#ifndef __QUATERNION_INCLUDED__
#define __QUATERNION_INCLUDED__

#define QUATERNION_IDENTITY float4(0, 0, 0, 1)

#ifndef PI
#define PI 3.14159265359f
#endif 

// Quaternion multiplication
// http://mathworld.wolfram.com/Quaternion.html
float4 qmul(float4 q1, float4 q2)
{
    return float4(
        q2.xyz * q1.w + q1.xyz * q2.w + cross(q1.xyz, q2.xyz),
        q1.w * q2.w - dot(q1.xyz, q2.xyz)
    );
}

// Vector rotation with a quaternion
// http://mathworld.wolfram.com/Quaternion.html
float3 rotate_vector(float3 v, float4 r)
{
    float4 r_c = r * float4(-1, -1, -1, 1);
    return qmul(r, qmul(float4(v, 0), r_c)).xyz;
}

// A given angle of rotation about a given axis
float4 rotate_angle_axis(float angle, float3 axis)
{
    float sn = sin(angle * 0.5);
    float cs = cos(angle * 0.5);
    return float4(axis * sn, cs);
}

// https://stackoverflow.com/questions/1171849/finding-quaternion-representing-the-rotation-from-one-vector-to-another
float4 from_to_rotation(float3 v1, float3 v2)
{
    float4 q;
    float d = dot(v1, v2);
    if (d < -0.999999)
    {
        float3 right = float3(1, 0, 0);
        float3 up = float3(0, 1, 0);
        float3 tmp = cross(right, v1);
        if (length(tmp) < 0.000001)
        {
            tmp = cross(up, v1);
        }
        tmp = normalize(tmp);
        q = rotate_angle_axis(PI, tmp);
    } else if (d > 0.999999) {
        q = QUATERNION_IDENTITY;
    } else {
        q.xyz = cross(v1, v2);
        q.w = 1 + d;
        q = normalize(q);
    }
    return q;
}

float4 q_conj(float4 q)
{
    return float4(-q.x, -q.y, -q.z, q.w);
}

// https://jp.mathworks.com/help/aeroblks/quaternioninverse.html
float4 q_inverse(float4 q)
{
    float4 conj = q_conj(q);
    return conj / (q.x * q.x + q.y * q.y + q.z * q.z + q.w * q.w);
}

float4 q_diff(float4 q1, float4 q2)
{
    return q2 * q_inverse(q1);
}

float4 q_look_at(float3 forward, float3 up)
{
    float3 right = normalize(cross(forward, up));
    up = normalize(cross(forward, right));

    float m00 = right.x;
    float m01 = right.y;
    float m02 = right.z;
    float m10 = up.x;
    float m11 = up.y;
    float m12 = up.z;
    float m20 = forward.x;
    float m21 = forward.y;
    float m22 = forward.z;

    float num8 = (m00 + m11) + m22;
    float4 q = QUATERNION_IDENTITY;
    if (num8 > 0.0)
    {
        float num = sqrt(num8 + 1.0);
        q.w = num * 0.5;
        num = 0.5 / num;
        q.x = (m12 - m21) * num;
        q.y = (m20 - m02) * num;
        q.z = (m01 - m10) * num;
        return q;
    }

    if ((m00 >= m11) && (m00 >= m22))
    {
        float num7 = sqrt(((1.0 + m00) - m11) - m22);
        float num4 = 0.5 / num7;
        q.x = 0.5 * num7;
        q.y = (m01 + m10) * num4;
        q.z = (m02 + m20) * num4;
        q.w = (m12 - m21) * num4;
        return q;
    }

    if (m11 > m22)
    {
        float num6 = sqrt(((1.0 + m11) - m00) - m22);
        float num3 = 0.5 / num6;
        q.x = (m10 + m01) * num3;
        q.y = 0.5 * num6;
        q.z = (m21 + m12) * num3;
        q.w = (m20 - m02) * num3;
        return q;
    }

    float num5 = sqrt(((1.0 + m22) - m00) - m11);
    float num2 = 0.5 / num5;
    q.x = (m20 + m02) * num2;
    q.y = (m21 + m12) * num2;
    q.z = 0.5 * num5;
    q.w = (m01 - m10) * num2;
    return q;
}

float4 q_slerp(in float4 a, in float4 b, float t)
{
    // if either input is zero, return the other.
    if (length(a) == 0.0)
    {
        if (length(b) == 0.0)
        {
            return QUATERNION_IDENTITY;
        }
        return b;
    }
    else if (length(b) == 0.0)
    {
        return a;
    }

    float cosHalfAngle = a.w * b.w + dot(a.xyz, b.xyz);

    if (cosHalfAngle >= 1.0 || cosHalfAngle <= -1.0)
    {
        return a;
    }
    else if (cosHalfAngle < 0.0)
    {
        b.xyz = -b.xyz;
        b.w = -b.w;
        cosHalfAngle = -cosHalfAngle;
    }

    float blendA;
    float blendB;
    if (cosHalfAngle < 0.99)
    {
        // do proper slerp for big angles
        float halfAngle = acos(cosHalfAngle);
        float sinHalfAngle = sin(halfAngle);
        float oneOverSinHalfAngle = 1.0 / sinHalfAngle;
        blendA = sin(halfAngle * (1.0 - t)) * oneOverSinHalfAngle;
        blendB = sin(halfAngle * t) * oneOverSinHalfAngle;
    }
    else
    {
        // do lerp if angle is really small.
        blendA = 1.0 - t;
        blendB = t;
    }

    float4 result = float4(blendA * a.xyz + blendB * b.xyz, blendA * a.w + blendB * b.w);
    if (length(result) > 0.0)
    {
        return normalize(result);
    }
    return QUATERNION_IDENTITY;
}

float4x4 quaternion_to_matrix(float4 quat)
{
    float4x4 m = float4x4(float4(0, 0, 0, 0), float4(0, 0, 0, 0), float4(0, 0, 0, 0), float4(0, 0, 0, 0));

    float x = quat.x, y = quat.y, z = quat.z, w = quat.w;
    float x2 = x + x, y2 = y + y, z2 = z + z;
    float xx = x * x2, xy = x * y2, xz = x * z2;
    float yy = y * y2, yz = y * z2, zz = z * z2;
    float wx = w * x2, wy = w * y2, wz = w * z2;

    m[0][0] = 1.0 - (yy + zz);
    m[0][1] = xy - wz;
    m[0][2] = xz + wy;

    m[1][0] = xy + wz;
    m[1][1] = 1.0 - (xx + zz);
    m[1][2] = yz - wx;

    m[2][0] = xz - wy;
    m[2][1] = yz + wx;
    m[2][2] = 1.0 - (xx + yy);

    m[3][3] = 1.0;

    return m;
}

// add http://edom18.hateblo.jp/entry/2018/06/25/084023#%E3%82%AA%E3%82%A4%E3%83%A9%E3%83%BC%E8%A7%92%E3%81%8B%E3%82%89%E3%82%AF%E3%82%A9%E3%83%BC%E3%82%BF%E3%83%8B%E3%82%AA%E3%83%B3%E3%82%92%E6%B1%82%E3%82%81%E3%82%8B
float4 euler_to_quaternion(float3 euler)
{
	float x = euler.x * (PI/180);
	float y = euler.y * (PI/180);
	float z = euler.z * (PI/180);

	float c1 = cos(x * 0.5);
	float c2 = cos(y * 0.5);
	float c3 = cos(z * 0.5);

	float s1 = sin(x * 0.5);
	float s2 = sin(y * 0.5);
	float s3 = sin(z * 0.5);
	
    float qx = s1 * c2 * c3 + c1 * s2 * s3;
    float qy = c1 * s2 * c3 - s1 * c2 * s3;
    float qz = c1 * c2 * s3 - s1 * s2 * c3;
    float qw = c1 * c2 * c3 + s1 * s2 * s3;

	return float4(qx, qy, qz, qw);
}

// add https://qiita.com/edo_m18/items/5db35b60112e281f840e
float3 quaternion_to_euler(float4 q)
{
	float x = q.x;
    float y = q.y;
    float z = q.z;
    float w = q.w;

    float x2 = x * x;
    float y2 = y * y;
    float z2 = z * z;

    float xy = x * y;
    float xz = x * z;
    float yz = y * z;
    float wx = w * x;
    float wy = w * y;
    float wz = w * z;

    // 1 - 2y^2 - 2z^2
    float m00 = 1 - (2 * y2) - (2 * z2);
    // 2xy + 2wz
    float m01 = (2 * xy) + (2 * wz);
    // 2xy - 2wz
    float m10 = (2 * xy) - (2 * wz);
    // 1 - 2x^2 - 2z^2
    float m11 = 1 - (2 * x2) - (2 * z2);
    // 2xz + 2wy
    float m20 = (2 * xz) + (2 * wy);
    // 2yz+2wx
    float m21 = (2 * yz) - (2 * wx);
    // 1 - 2x^2 - 2y^2
    float m22 = 1 - (2 * x2) - (2 * y2);

    float tx, ty, tz;

    if ((int) m21 == 1)
    {
        tx = PI / 2;
        ty = 0;
        tz = atan2(m10, m00);
    }
    else if ((int) m21 == -1)
    {
        tx = -PI / 2;
        ty = 0;
        tz = atan2(m10, m00);
    }
    else
    {
        tx = asin(-m21);
        ty = atan2(m20, m22);
        tz = atan2(m01, m11);
    }
	return float3(tx, ty, tz);
}


float3 rotate_quaternion_axis(float3 n, float rot, float3 center, float3 pos)
{
	float4 q = 0;
	q.x = cos(rot/2);
	q.y = n.x * sin(rot/2);
	q.z = n.y * sin(rot/2);
	q.w = n.z * sin(rot/2);

	float3x3 r = float3x3(
		q.x*q.x + q.y*q.y - q.z*q.z - q.w*q.w, 2*(q.y*q.z-q.x*q.w), 2*(q.y*q.w + q.x*q.z),
		2*(q.y*q.z+q.x*q.w), q.x*q.x - q.y*q.y + q.z*q.z - q.w*q.w, 2*(q.z*q.w - q.x*q.y),
		2*(q.y*q.w-q.x*q.z), 2*(q.z*q.w+q.x*q.y), q.x*q.x - q.y*q.y - q.z*q.z + q.w*q.w
	);

	return mul(r, pos-center) + center;
}

#endif // __QUATERNION_INCLUDED__

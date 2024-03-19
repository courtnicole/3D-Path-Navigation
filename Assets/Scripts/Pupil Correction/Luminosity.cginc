#define LINEAR_TO_XYZ_X float3(0.4124564, 0.3575761, 0.1804375)
#define LINEAR_TO_XYZ_Y float3(0.2126729, 0.7151522, 0.0721750)
#define LINEAR_TO_XYZ_Z float3(0.0193339, 0.1191920, 0.9503041)

/*
 * Assuming D65 white point:
 * 
 * X: 0.4124564  0.3575761  0.1804375
 * Y: 0.2126729  0.7151522  0.0721750
 * Z: 0.0193339  0.1191920  0.9503041
 */
float3 get_linear_to_xyz(const float3 color)
{
    return float3(
        dot(color, LINEAR_TO_XYZ_X),
        dot(color, LINEAR_TO_XYZ_Y),
        dot(color, LINEAR_TO_XYZ_Z)
    );
}

float2 get_fixation_point(float3 normalized_gaze_direction, const float tan_half_horizontal_fov, const float tan_half_vertical_fov)
{
    float2 gaze_point;
    gaze_point.x = (normalized_gaze_direction.x / normalized_gaze_direction.z) / tan_half_horizontal_fov;
    gaze_point.y = (normalized_gaze_direction.y / normalized_gaze_direction.z) / tan_half_vertical_fov;
    gaze_point.x = -gaze_point.x;

    gaze_point = (gaze_point + float2(1.0,1.0)) / 2.0f;
    return gaze_point;
}

bool inside_fovea(const float2 fixation_point, const float2 pixel_position, const float radius_sq)
{
    const float2 diff = fixation_point - pixel_position;
    return dot(diff, diff) < radius_sq;
}


//SV_GroupThreadID: which thread we're in inside the current scope (group). Range defined by numthreads declaration => **AGNOSTIC OF DISPATCH SIZE**
//SV_GroupID: which group we're in inside the current scope (dispatch). Should be named SV_DispatchGroupID.
//SV_DispatchThreadID: which thread we're in inside the current scope (dispatch). Flatten for unique index!
//SV_GroupIndex: SHOULD BE NAMED SV_DispatchGroupThreadID. SV_GroupThreadID.x + SV_GroupThreadID.y * threads.x + SV_GroupThreadID.z * threads.x * threads.y
// ^^useful for indexing into groupshared memory

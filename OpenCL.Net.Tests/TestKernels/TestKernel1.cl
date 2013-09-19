#define normalize(pt) (float2)(642.2889f / pt.x, 192.5287f / pt.y)
const sampler_t damageSurfaceSampler = CLK_NORMALIZED_COORDS_TRUE | CLK_ADDRESS_CLAMP_TO_EDGE | CLK_FILTER_LINEAR;


kernel void DamageSurfaceSample(read_only image2d_t damageSurface, global float2* points, global float* results, local float4* localBuffer )
{
	int tid = get_global_id(0);
	float2 point = points[tid];
	float2 normalized = normalize(point);
	float4 dSurfValue = read_imagef(damageSurface, damageSurfaceSampler, normalized);
	
	results[tid] = dSurfValue.w;
}
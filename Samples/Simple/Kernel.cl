__kernel void doSomething(__global float* a, __global float* b, __local uchar3* local, float scale)
{
	int id = get_global_id(0);
	b[id] = a[id] * scale;
}
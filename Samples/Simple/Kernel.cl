__kernel void doSomething(
					      /* This is A */ __global float* a, 
						  /* This is B */ __global float* b, 
						  // Scaling factor here
						  float scale)
{
	int id = get_global_id(0);
	b[id] = a[id] * scale;
}
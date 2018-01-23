#define N 4

float4 
basisVector(float t, float t0, float t1)
{
    float h = t1 - t0;
    float t_min_t0 = t - t0;
    float t_min_t1 = t - t1;
    float sqr_t_min_t0 = t_min_t0*t_min_t0;
    float sqr_t_min_t1 = t_min_t1*t_min_t1;
    float sqr_h = h*h;
    float4 m = (float4)(
        (1 + 2*t_min_t0/h)*sqr_t_min_t1/sqr_h,
        (1 - 2*t_min_t1/h)*sqr_t_min_t0/sqr_h,
        t_min_t0*sqr_t_min_t1/sqr_h,
        sqr_t_min_t0*t_min_t1/sqr_h);
    return m;
}

float
multiplyBasis(float4* xVec,local float* basis, float4* yVec)
{
    int N2 = 2*N; int N3 = 3*N;

    float z = 0;
    //result = lPhi.s1 * yVec
    for(int k = 0; k < N; k++)
    {
        float lm = xVec[0].s0*basis[k]
            +xVec[0].s1*basis[N+k]
            +xVec[0].s2*basis[N2+k]
            +xVec[0].s3*basis[N3+k];
				switch(k)
				{
						case 0:
							z += lm * yVec[0].s0; 
							break;
						case 1:
							z += lm * yVec[0].s1; 
							break;
						case 2:
							z += lm * yVec[0].s2; 
							break;
						default:
							z += lm * yVec[0].s3; 
							break;
				}
        
    }
    return z;
}

__kernel void
bicubicHermiteSegment(global float4* result,
                constant float* u0, 
                constant float* u1, 
                constant float* v0, 
                constant float* v1, 
								constant float* density, 
                global float* basis,
				local float* basisLocal
                )
{
    
    float uDis = fabs(*u1-*u0);
    float vDis = fabs(*v1-*v0);
    
    int count = get_global_size(0);
    
    int i = get_global_id(0);
    int j = get_global_id(1);
    int xCount = get_global_size(0);
    int yCount = get_global_size(1);
	
		float lDensity = *density;

    float x = *u0 + i*lDensity;
    float y = *v0 + j*lDensity;
            
    float4 basisX = basisVector(x,*u0,*u1);
    float4 basisY = basisVector(y,*v0,*v1);
    
		int k=0;
		for(k=0;k<16;k++)
		{
				basisLocal[k] = basis[k];
		}
		barrier(CLK_LOCAL_MEM_FENCE);
    
    float z = multiplyBasis(&basisX, basisLocal, &basisY);
    result[j + i*yCount] = (float4)(x,y,z,1.0f);
          

}
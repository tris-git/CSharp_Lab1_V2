#include "pch.h"
#include "framework.h"
#include "mathlibrary.h"
#include "time.h"
#include <cmath>
//#include "mkl.h"

float my_logf(float);

/* compute inverse error functions with maximum error of 2.35793 ulp */
double my_erfinvf(float a)
{
    float p, r, t;
    t = fmaf(a, 0.0f - a, 1.0f);
    t = my_logf(t);
    if (fabsf(t) > 6.125f) { // maximum ulp error = 2.35793
        p = 3.03697567e-10f; //  0x1.4deb44p-32 
        p = fmaf(p, t, 2.93243101e-8f); //  0x1.f7c9aep-26 
        p = fmaf(p, t, 1.22150334e-6f); //  0x1.47e512p-20 
        p = fmaf(p, t, 2.84108955e-5f); //  0x1.dca7dep-16 
        p = fmaf(p, t, 3.93552968e-4f); //  0x1.9cab92p-12 
        p = fmaf(p, t, 3.02698812e-3f); //  0x1.8cc0dep-9 
        p = fmaf(p, t, 4.83185798e-3f); //  0x1.3ca920p-8 
        p = fmaf(p, t, -2.64646143e-1f); // -0x1.0eff66p-2 
        p = fmaf(p, t, 8.40016484e-1f); //  0x1.ae16a4p-1 
    }
    else { // maximum ulp error = 2.35002
        p = 5.43877832e-9f;  //  0x1.75c000p-28 
        p = fmaf(p, t, 1.43285448e-7f); //  0x1.33b402p-23 
        p = fmaf(p, t, 1.22774793e-6f); //  0x1.499232p-20 
        p = fmaf(p, t, 1.12963626e-7f); //  0x1.e52cd2p-24 
        p = fmaf(p, t, -5.61530760e-5f); // -0x1.d70bd0p-15 
        p = fmaf(p, t, -1.47697632e-4f); // -0x1.35be90p-13 
        p = fmaf(p, t, 2.31468678e-3f); //  0x1.2f6400p-9 
        p = fmaf(p, t, 1.15392581e-2f); //  0x1.7a1e50p-7 
        p = fmaf(p, t, -2.32015476e-1f); // -0x1.db2aeep-3 
        p = fmaf(p, t, 8.86226892e-1f); //  0x1.c5bf88p-1 
    }
    r = a * p;
    return (double)r;
}

/* compute natural logarithm with a maximum error of 0.85089 ulp */
float my_logf(float a)
{
    float i, m, r, s, t;
    int e;

    m = frexpf(a, &e);
    if (m < 0.666666667f) { // 0x1.555556p-1
        m = m + m;
        e = e - 1;
    }
    i = (float)e;
    /* m in [2/3, 4/3] */
    m = m - 1.0f;
    s = m * m;
    /* Compute log1p(m) for m in [-1/3, 1/3] */
    r = -0.130310059f;  // -0x1.0ae000p-3
    t = 0.140869141f;  //  0x1.208000p-3
    r = fmaf(r, s, -0.121484190f); // -0x1.f19968p-4
    t = fmaf(t, s, 0.139814854f); //  0x1.1e5740p-3
    r = fmaf(r, s, -0.166846052f); // -0x1.55b362p-3
    t = fmaf(t, s, 0.200120345f); //  0x1.99d8b2p-3
    r = fmaf(r, s, -0.249996200f); // -0x1.fffe02p-3
    r = fmaf(t, m, r);
    r = fmaf(r, m, 0.333331972f); //  0x1.5554fap-2
    r = fmaf(r, m, -0.500000000f); // -0x1.000000p-1
    r = fmaf(r, s, m);
    r = fmaf(i, 0.693147182f, r); //  0x1.62e430p-1 // log(2)
    if (!((a > 0.0f) && (a <= 3.40282346e+38f))) { // 0x1.fffffep+127
        r = a + a;  // silence NaNs if necessary
        if (a < 0.0f) r = NAN; //  NaN
        if (a == 0.0f) r = INFINITE; // -Inf
    }
    return r;
}

// Функция-заглушка
void somefunc(int n, double* vector, double* args, int vmf_num)
{
    if (vmf_num == 0) {
        for (int i = 0; i < n; i++) {
            vector[i] = tan(args[i]) + rand() % 13;
        }
    }
    else {
        for (int i = 0; i < n; i++) {
            vector[i] = my_erfinvf(args[i]) + rand() % 17;
        }
    }
}

extern "C" _declspec(dllexport)
void vmd(int n, double min, double max, int vmf_num, int time[], double acc[])
{
    double* args = new double[n];
    for (int i = 0; i < n; i++) {
        args[i] = (max - min) / n * i + min;
    }
    double* value_HA = new double[n];
    double* value_EP = new double[n];
    double* value_C = new double[n];

    if (vmf_num == 0) {
        unsigned int start_time = clock();
        //vmdTan(n, args, value_HA, VML_HA);
        //value_HA 
        somefunc(n, value_HA, args, vmf_num);
        unsigned int end_time = clock();
        time[0] = end_time - start_time;

        start_time = clock();
        //vmdTan(n, args, value_EP, VML_EP);
        //value_EP
        somefunc(n, value_EP, args, vmf_num);
        end_time = clock();
        time[1] = end_time - start_time;

        start_time = clock();
        for (int i = 0; i < n; i++) {
            value_C[i] = tan(args[i]);
        }
        end_time = clock();
        time[2] = end_time - start_time;
    }
    else {
        unsigned int start_time = clock();
        //vmdErfInv(n, args, value_HA, VML_HA);
        //value_HA
        somefunc(n, value_HA, args, vmf_num);
        unsigned int end_time = clock();
        time[0] = end_time - start_time;

        start_time = clock();
        //vmdErfInv(n, args, value_EP, VML_EP);
        //value_EP
        somefunc(n, value_EP, args, vmf_num);
        end_time = clock();
        time[1] = end_time - start_time;

        start_time = clock();
        for (int i = 0; i < n; i++) {
            value_C[i] = my_erfinvf(args[i]);
        }
        end_time = clock();
        time[2] = end_time - start_time;
    }

    double max_diff = -1;
    for (int i = 0; i < n; i++) {
        if (abs(value_HA[i] - value_EP[i]) > max_diff) {
            max_diff = abs(value_HA[i] - value_EP[i]);
            acc[0] = value_HA[i];
            acc[1] = value_EP[i];
            acc[2] = args[i];
        }
    }

    delete[] args;
    delete[] value_HA;
    delete[] value_EP;
    delete[] value_C;
}
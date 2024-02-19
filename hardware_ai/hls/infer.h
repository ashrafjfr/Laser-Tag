#include "hls_stream.h"
#include <cmath>


#ifndef MLP_H
#define MLP_H

#define in_n 300
#define l1_n 64
#define l2_n 40
#define l3_n 20
#define l4_n 10
#define out_n 9

struct AXIS_IO{
	float data;
	int last;
};

typedef hls::stream<AXIS_IO> stream_io;

// Function prototypes
void l1(const float (&input)[in_n], float &output, const int index);
void l2(const float (&input)[l1_n], float &output, const int index);
void l3(const float (&input)[l2_n], float &output, const int index);
void l4(const float (&input)[l3_n], float &output, const int index);
void l5(const float (&input)[l4_n], float &output, const int index);
void softmax(float (&input)[out_n]);
void inference(stream_io &s_axis, stream_io &m_axis);

#endif // MLP_H

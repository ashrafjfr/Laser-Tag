#include "infer.h"
#include "w.h"
/**
 * This represents the first layer of MLP
 * @param input[in_n] the input vector
 * @param output[out_n] the output vector
 * @param index the index of the output neuron
 * @return void
*/
void l1(const float (&input)[in_n], float &output, const int index) {
        float bias = b1[index]; 

        for (int i = 0; i < in_n; i++) {
#pragma HLS PIPELINE
#pragma HLS unroll factor = 2
                bias += (input[i] * w1[i][index]);
        }

        // ReLU
        output = (bias > 0) ? bias : 0;
    }

void l2(const float (&input)[l1_n], float &output, const int index) {
        float bias = b2[index];
        for (int i = 0; i < l1_n; i++) {
#pragma HLS PIPELINE
#pragma HLS unroll factor = 2
                bias += (input[i] * w2[i][index]);
        }

        output = (bias > 0) ? bias : 0;
}

void l3(const float (&input)[l2_n], float &output, const int index) {
        float bias = b3[index];
        for (int i = 0; i < l2_n; i++) {
#pragma HLS PIPELINE
#pragma HLS unroll factor = 2
                bias += (input[i] * w3[i][index]);
        }

        output = (bias > 0) ? bias : 0;
}


void l4(const float (&input)[l3_n], float &output, const int index) {
        float bias = b4[index];
        for (int i = 0; i < l3_n; i++) {
#pragma HLS PIPELINE
#pragma HLS unroll factor = 2
                bias += (input[i] * w4[i][index]);
        }

        output = bias;
}

void l5(const float (&input)[l4_n], float &output, const int index) {
        float bias = b5[index];
        for (int i = 0; i < l4_n; i++) {
#pragma HLS PIPELINE
#pragma HLS unroll factor = 2
                bias += (input[i] * w5[i][index]);
        }

        output = bias;
}

void softmax(float (&input)[out_n]) {
        float sum = 0;
        for (int i = 0; i < out_n; i++) {
#pragma HLS PIPELINE
#pragma HLS unroll factor = 2
                sum += exp(input[i]);
        }

        for (int i = 0; i < out_n; i++) {
#pragma HLS PIPELINE
#pragma HLS UNROLL
                input[i] = (exp(input[i]) / sum);
        }
}


void inference(stream_io& s_axis, stream_io& m_axis) {

    #pragma HLS INTERFACE axis port=s_axis
    #pragma HLS INTERFACE axis port=m_axis
	#pragma HLS INTERFACE ap_ctrl_none port=return

        float input[in_n];      //300
        float l1_out[l1_n];     //64
        float l2_out[l2_n];     //40
        float l3_out[l3_n];     //20
        float l4_out[l4_n];     //10
        float output[out_n];    //9

        AXIS_IO in;
        AXIS_IO out;

        // read input
        for (int i = 0; i < in_n; i++) {
#pragma HLS PIPELINE
#pragma HLS unroll factor = 2
                in = s_axis.read();
                input[i] = in.data;
        }

        // layer 1
        for (int i = 0; i < l1_n; i++) {
                l1(input, l1_out[i], i);
        }

        // layer 2
        for (int i = 0; i < l2_n; i++) {
                l2(l1_out, l2_out[i], i);
        }

        // layer 3
        for (int i = 0; i < l3_n; i++) {
                l3(l2_out, l3_out[i], i);
        }

        // layer 4
        for (int i = 0; i < l4_n; i++) {
                l4(l3_out, l4_out[i], i);
        }

        // layer 5
        for (int i = 0; i < out_n; i++) {
                l5(l4_out, output[i], i);
        }

        // softmax
        softmax(output);

        // write output
        int action = 0;
        float max = output[0];

        for (int i = 1; i < out_n; i++) {
                if (output[i] > max) {
                        max = output[i];
                        action = i;
                }
                if (i == out_n - 1) {
                	out.last = 1;	//tell compiler TLAST
                }
        }

        out.last = 1;
        out.data = action;
        m_axis.write(out);
}

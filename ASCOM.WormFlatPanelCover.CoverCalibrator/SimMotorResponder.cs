using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASCOM.WormFlatPanelCover
{
    class SimResponse
    {
        public int angle = -333;
        public byte[] response = null;
    }
    class SimAckPair
    {
        public enum REQUEST_TYPE { 
            INVALID, STOP_MOTOR, START_MOTOR, VOID_MOTOR, 
            SET_INIT_PARAM, SET_OPEN_PARAM, SET_CLOSE_PARAM,
            READ_MOTOR_OPEN, READ_MOTOR_CLOSE };

        public REQUEST_TYPE request_type = REQUEST_TYPE.INVALID;
        public byte[] request = null;

        public List<SimResponse> responses = new List<SimResponse>();
        public int last_sent = 0;

        public bool isMatch(byte[] input, REQUEST_TYPE type)
        {
            if (type == request_type && isMatch(input))
                return true;
            return false;
        }
        public bool isMatch(byte[] input)
        {
            if (input.Length == request.Length)
            {
                for (int i = 0; i < request.Length; i++)
                    if (input[i] != request[i])
                        return false;
                return true;
            }
            return false;
        }
        public bool isMatchOpenCloseParam(byte[] input)
        {
            if (input.Length < 10)
                return false;
            if ((input[0] == 0xB0) && (input[1] == 0x62) && (input[7] == 0xC8) && (input[8] == 0x0A))
                return true;
            return false;
        }
    }

    class SimMotorResponder
    {
        CoverCalibrator driver = null;
        List<SimAckPair> simulator_data = new List<SimAckPair>();
        List<int> open_angles = new List<int>();
        List<int> close_angles = new List<int>();

        SimAckPair.REQUEST_TYPE read_mode = SimAckPair.REQUEST_TYPE.INVALID;
        
        protected byte[] last_request = null;

        public SimMotorResponder(CoverCalibrator drv)
        {
            driver = drv;

            initialize_close_angles();
            initialize_open_angles();
            simulator_data.Clear();
            initSimulatorData();
        }

        public void setLastRequest(byte[] data)
        {
            last_request = data;
        }

        public byte[] getResponse()
        {
            if (last_request == null)
                return null;

            byte[] input = last_request;

            foreach (SimAckPair item in simulator_data)
            {
                if (item.isMatch(input))
                {
                    if (item.request_type == SimAckPair.REQUEST_TYPE.VOID_MOTOR ||
                        item.request_type == SimAckPair.REQUEST_TYPE.STOP_MOTOR ||
                        item.request_type == SimAckPair.REQUEST_TYPE.START_MOTOR ||
                        item.request_type == SimAckPair.REQUEST_TYPE.SET_INIT_PARAM)
                    {
                        return item.responses[0].response;
                    }
                }
                if (item.isMatchOpenCloseParam(input))
                {
                    //  found set parameter command for open or close
                    //  now decode travel distance
                    byte[] calc_angle = new byte[4];
                    calc_angle[0] = input[5];
                    calc_angle[1] = input[4];
                    calc_angle[2] = input[3];
                    calc_angle[3] = input[2];
                    int req_angle = System.BitConverter.ToInt32(calc_angle, 0) / 8;

                    if (req_angle > 0)
                    {
                        SimAckPair readpair = findAckPairByType(SimAckPair.REQUEST_TYPE.READ_MOTOR_OPEN);
                        read_mode = SimAckPair.REQUEST_TYPE.READ_MOTOR_OPEN;
                        //  use angle to find starting open point
                        int search_angle = driver.targetAngle - req_angle;
                        for (int i = 1; i < readpair.responses.Count; i++)
                        {
                            if (readpair.responses[i - 1].angle <= search_angle && readpair.responses[i].angle >= search_angle)
                            {
                                readpair.last_sent = i - 1;
                                break;
                            }
                        }
                        return item.responses[0].response;
                    }
                    else
                    {
                        SimAckPair readpair = findAckPairByType(SimAckPair.REQUEST_TYPE.READ_MOTOR_CLOSE);
                        read_mode = SimAckPair.REQUEST_TYPE.READ_MOTOR_CLOSE;
                        //  use angle to find starting close point
                        int search_angle = -req_angle;
                        for (int i = 1; i < readpair.responses.Count; i++)
                        {
                            if (readpair.responses[i - 1].angle >= search_angle && readpair.responses[i].angle <= search_angle)
                            {
                                readpair.last_sent = i - 1;
                                break;
                            }
                        }
                        return item.responses[0].response;
                    }
                }
                if (item.isMatch(input, read_mode))
                {
                    return item.responses[item.last_sent++].response;
                }

            }
            return null;
        }

/*
*/

        SimAckPair findAckPairByType(SimAckPair.REQUEST_TYPE type)
        {
            foreach (SimAckPair item in simulator_data)
            {
                if (type == item.request_type)
                    return item;
            }
            return null;
        }
        void initSimulatorData()
        {
            List<byte[]> raw_list = new List<byte[]>();
            byte[] tmparray = null;
            SimResponse simrsp = null;

            //  initial parameters
            SimAckPair pair = new SimAckPair();
            tmparray = new byte[] { 0xB0, 0x62, 0x00, 0x00, 0x00, 0xFF, 0x00, 0x0F, 0x05, 0x25 };
            pair.request_type = SimAckPair.REQUEST_TYPE.SET_INIT_PARAM;
            pair.request = tmparray;
            tmparray = new byte[] { 0x61, 0x11, 0x00 };
            simrsp = new SimResponse();
            simrsp.response = tmparray;
            pair.responses.Add(simrsp);
            simulator_data.Add(pair);

            //  stop step motor
            pair = new SimAckPair();
            tmparray = new byte[] { 0xB0, 0x61, 0x00, 0x11 };
            pair.request_type = SimAckPair.REQUEST_TYPE.STOP_MOTOR;
            pair.request = tmparray;
            tmparray = new byte[] { 0xB0, 0x00, 0x00 }; raw_list.Add(tmparray);
            simrsp = new SimResponse();
            simrsp.response = tmparray;
            pair.responses.Add(simrsp);
            simulator_data.Add(pair);

            //  start step motor
            pair = new SimAckPair();
            tmparray = new byte[] { 0xB0, 0x61, 0x01, 0x12 };
            pair.request_type = SimAckPair.REQUEST_TYPE.START_MOTOR;
            pair.request = tmparray;
            tmparray = new byte[] { 0xB0, 0x00, 0x00 }; raw_list.Add(tmparray);
            simrsp = new SimResponse();
            simrsp.response = tmparray;
            pair.responses.Add(simrsp);
            simulator_data.Add(pair);

            //  void step motor drive
            pair = new SimAckPair();
            tmparray = new byte[] { 0xB0, 0x61, 0x03, 0x14 }; raw_list.Add(tmparray);
            pair.request_type = SimAckPair.REQUEST_TYPE.VOID_MOTOR;
            pair.request = tmparray;
            tmparray = new byte[] { 0xB0, 0x11, 0x00 }; raw_list.Add(tmparray);
            simrsp = new SimResponse();
            simrsp.response = tmparray;
            pair.responses.Add(simrsp);
            simulator_data.Add(pair);
            
            //  open cover (set open angle 525, speed 200, acceleration 10)
            pair = new SimAckPair();
            tmparray = new byte[] { 0xB0, 0x62, 0x00, 0x00, 0x10, 0x40, 0x00, 0xC8, 0x0A, 0x34 };
            pair.request_type = SimAckPair.REQUEST_TYPE.SET_OPEN_PARAM;
            pair.request = tmparray;
            tmparray = new byte[] { 0xB0, 0x00, 0x00 };
            simrsp = new SimResponse();
            simrsp.response = tmparray;
            pair.responses.Add(simrsp);
            simulator_data.Add(pair);

            //  open cover, read step motor control data
            raw_list.Clear();
            tmparray = new byte[] { 0xB0, 0x63, 0x13 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0xA3, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x09, 0x5D }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0x63, 0x13 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0xA3, 0x01, 0x00, 0x00, 0x00, 0x03, 0x00, 0x1B, 0x09, 0x7B }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0x63, 0x13 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0xA3, 0x01, 0x00, 0x00, 0x00, 0x11, 0x00, 0x99, 0x09, 0x07 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0x63, 0x13 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0xA3, 0x01, 0x00, 0x00, 0x00, 0x3A, 0x00, 0xC8, 0x09, 0x5F }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0x63, 0x13 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0xA3, 0x01, 0x00, 0x00, 0x00, 0x63, 0x00, 0xC8, 0x09, 0x88 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0x63, 0x13 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0xA3, 0x01, 0x00, 0x00, 0x00, 0x8B, 0x00, 0xC8, 0x09, 0xB0 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0x63, 0x13 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0xA3, 0x01, 0x00, 0x00, 0x00, 0xB5, 0x00, 0xC8, 0x09, 0xDA }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0x63, 0x13 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0xA3, 0x01, 0x00, 0x00, 0x00, 0xDE, 0x00, 0xC8, 0x09, 0x03 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0x63, 0x13 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0xA3, 0x01, 0x00, 0x00, 0x01, 0x07, 0x00, 0xC8, 0x09, 0x2D }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0x63, 0x13 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0xA3, 0x01, 0x00, 0x00, 0x01, 0x30, 0x00, 0xC8, 0x09, 0x56 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0x63, 0x13 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0xA3, 0x01, 0x00, 0x00, 0x01, 0x59, 0x00, 0xC8, 0x09, 0x7F }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0x63, 0x13 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0xA3, 0x01, 0x00, 0x00, 0x01, 0x83, 0x00, 0xC8, 0x09, 0xA9 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0x63, 0x13 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0xA3, 0x01, 0x00, 0x00, 0x01, 0xAC, 0x00, 0xC8, 0x09, 0xD2 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0x63, 0x13 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0xA3, 0x01, 0x00, 0x00, 0x01, 0xD6, 0x00, 0xC8, 0x09, 0xFC }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0x63, 0x13 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0xA3, 0x01, 0x00, 0x00, 0x01, 0xFE, 0x00, 0xC8, 0x09, 0x24 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0x63, 0x13 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0xA3, 0x01, 0x00, 0x00, 0x02, 0x27, 0x00, 0xC8, 0x09, 0x4E }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0x63, 0x13 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0xA3, 0x01, 0x00, 0x00, 0x02, 0x51, 0x00, 0xC8, 0x09, 0x78 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0x63, 0x13 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0xA3, 0x01, 0x00, 0x00, 0x02, 0x7A, 0x00, 0xC8, 0x09, 0xA1 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0x63, 0x13 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0xA3, 0x01, 0x00, 0x00, 0x02, 0xA4, 0x00, 0xC8, 0x09, 0xCB }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0x63, 0x13 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0xA3, 0x01, 0x00, 0x00, 0x02, 0xCE, 0x00, 0xC8, 0x09, 0xF5 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0x63, 0x13 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0xA3, 0x01, 0x00, 0x00, 0x02, 0xF8, 0x00, 0xC8, 0x09, 0x1F }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0x63, 0x13 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0xA3, 0x01, 0x00, 0x00, 0x03, 0x22, 0x00, 0xC8, 0x09, 0x4A }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0x63, 0x13 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0xA3, 0x01, 0x00, 0x00, 0x03, 0x4A, 0x00, 0xC8, 0x09, 0x72 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0x63, 0x13 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0xA3, 0x01, 0x00, 0x00, 0x03, 0x74, 0x00, 0xC8, 0x09, 0x9C }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0x63, 0x13 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0xA3, 0x01, 0x00, 0x00, 0x03, 0x9E, 0x00, 0xC8, 0x09, 0xC6 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0x63, 0x13 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0xA3, 0x01, 0x00, 0x00, 0x03, 0xC8, 0x00, 0xC8, 0x09, 0xF0 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0x63, 0x13 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0xA3, 0x01, 0x00, 0x00, 0x03, 0xF0, 0x00, 0xC8, 0x09, 0x18 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0x63, 0x13 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0xA3, 0x01, 0x00, 0x00, 0x04, 0x1A, 0x00, 0xC8, 0x09, 0x43 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0x63, 0x13 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0xA3, 0x01, 0x00, 0x00, 0x04, 0x44, 0x00, 0xC8, 0x09, 0x6D }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0x63, 0x13 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0xA3, 0x01, 0x00, 0x00, 0x04, 0x6E, 0x00, 0xC8, 0x09, 0x97 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0x63, 0x13 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0xA3, 0x01, 0x00, 0x00, 0x04, 0x98, 0x00, 0xC8, 0x09, 0xC1 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0x63, 0x13 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0xA3, 0x01, 0x00, 0x00, 0x04, 0xC2, 0x00, 0xC8, 0x09, 0xEB }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0x63, 0x13 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0xA3, 0x01, 0x00, 0x00, 0x04, 0xEB, 0x00, 0xC8, 0x09, 0x14 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0x63, 0x13 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0xA3, 0x01, 0x00, 0x00, 0x05, 0x14, 0x00, 0xC8, 0x09, 0x3E }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0x63, 0x13 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0xA3, 0x01, 0x00, 0x00, 0x05, 0x3D, 0x00, 0xC8, 0x09, 0x67 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0x63, 0x13 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0xA3, 0x01, 0x00, 0x00, 0x05, 0x66, 0x00, 0xC8, 0x09, 0x90 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0x63, 0x13 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0xA3, 0x01, 0x00, 0x00, 0x05, 0x8E, 0x00, 0xC8, 0x09, 0xB8 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0x63, 0x13 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0xA3, 0x01, 0x00, 0x00, 0x05, 0xB7, 0x00, 0xC8, 0x09, 0xE1 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0x63, 0x13 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0xA3, 0x01, 0x00, 0x00, 0x05, 0xDF, 0x00, 0xC8, 0x09, 0x09 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0x63, 0x13 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0xA3, 0x01, 0x00, 0x00, 0x06, 0x08, 0x00, 0xC8, 0x09, 0x33 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0x63, 0x13 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0xA3, 0x01, 0x00, 0x00, 0x06, 0x31, 0x00, 0xC8, 0x09, 0x5C }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0x63, 0x13 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0xA3, 0x01, 0x00, 0x00, 0x06, 0x5A, 0x00, 0xC8, 0x09, 0x85 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0x63, 0x13 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0xA3, 0x01, 0x00, 0x00, 0x06, 0x84, 0x00, 0xC8, 0x09, 0xAF }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0x63, 0x13 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0xA3, 0x01, 0x00, 0x00, 0x06, 0xAE, 0x00, 0xC8, 0x09, 0xD9 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0x63, 0x13 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0xA3, 0x01, 0x00, 0x00, 0x06, 0xD7, 0x00, 0xC8, 0x09, 0x02 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0x63, 0x13 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0xA3, 0x01, 0x00, 0x00, 0x07, 0x01, 0x00, 0xC8, 0x09, 0x2D }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0x63, 0x13 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0xA3, 0x01, 0x00, 0x00, 0x07, 0x29, 0x00, 0xC8, 0x09, 0x55 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0x63, 0x13 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0xA3, 0x01, 0x00, 0x00, 0x07, 0x53, 0x00, 0xC8, 0x09, 0x7F }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0x63, 0x13 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0xA3, 0x01, 0x00, 0x00, 0x07, 0x7D, 0x00, 0xC8, 0x09, 0xA9 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0x63, 0x13 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0xA3, 0x01, 0x00, 0x00, 0x07, 0xA7, 0x00, 0xC8, 0x09, 0xD3 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0x63, 0x13 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0xA3, 0x01, 0x00, 0x00, 0x07, 0xD1, 0x00, 0xC8, 0x09, 0xFD }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0x63, 0x13 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0xA3, 0x01, 0x00, 0x00, 0x07, 0xFA, 0x00, 0xC8, 0x09, 0x26 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0x63, 0x13 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0xA3, 0x01, 0x00, 0x00, 0x08, 0x23, 0x00, 0xC8, 0x09, 0x50 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0x63, 0x13 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0xA3, 0x01, 0x00, 0x00, 0x08, 0x4C, 0x00, 0xC8, 0x09, 0x79 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0x63, 0x13 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0xA3, 0x01, 0x00, 0x00, 0x08, 0x75, 0x00, 0xC8, 0x09, 0xA2 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0x63, 0x13 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0xA3, 0x01, 0x00, 0x00, 0x08, 0x9D, 0x00, 0xC8, 0x09, 0xCA }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0x63, 0x13 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0xA3, 0x01, 0x00, 0x00, 0x08, 0xC7, 0x00, 0xC8, 0x09, 0xF4 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0x63, 0x13 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0xA3, 0x01, 0x00, 0x00, 0x08, 0xF1, 0x00, 0xC8, 0x09, 0x1E }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0x63, 0x13 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0xA3, 0x01, 0x00, 0x00, 0x09, 0x1B, 0x00, 0xC8, 0x09, 0x49 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0x63, 0x13 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0xA3, 0x01, 0x00, 0x00, 0x09, 0x45, 0x00, 0xC8, 0x09, 0x73 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0x63, 0x13 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0xA3, 0x01, 0x00, 0x00, 0x09, 0x6F, 0x00, 0xC8, 0x09, 0x9D }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0x63, 0x13 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0xA3, 0x01, 0x00, 0x00, 0x09, 0x98, 0x00, 0xC8, 0x09, 0xC6 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0x63, 0x13 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0xA3, 0x01, 0x00, 0x00, 0x09, 0xC1, 0x00, 0xC8, 0x09, 0xEF }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0x63, 0x13 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0xA3, 0x01, 0x00, 0x00, 0x09, 0xEA, 0x00, 0xC8, 0x09, 0x18 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0x63, 0x13 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0xA3, 0x01, 0x00, 0x00, 0x0A, 0x13, 0x00, 0xC8, 0x09, 0x42 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0x63, 0x13 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0xA3, 0x01, 0x00, 0x00, 0x0A, 0x3B, 0x00, 0xC8, 0x09, 0x6A }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0x63, 0x13 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0xA3, 0x01, 0x00, 0x00, 0x0A, 0x64, 0x00, 0xC8, 0x09, 0x93 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0x63, 0x13 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0xA3, 0x01, 0x00, 0x00, 0x0A, 0x8E, 0x00, 0xC8, 0x09, 0xBD }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0x63, 0x13 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0xA3, 0x01, 0x00, 0x00, 0x0A, 0xB7, 0x00, 0xC8, 0x09, 0xE6 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0x63, 0x13 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0xA3, 0x01, 0x00, 0x00, 0x0A, 0xE0, 0x00, 0xC8, 0x09, 0x0F }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0x63, 0x13 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0xA3, 0x01, 0x00, 0x00, 0x0B, 0x09, 0x00, 0xC8, 0x09, 0x39 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0x63, 0x13 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0xA3, 0x01, 0x00, 0x00, 0x0B, 0x32, 0x00, 0xC8, 0x09, 0x62 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0x63, 0x13 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0xA3, 0x01, 0x00, 0x00, 0x0B, 0x5B, 0x00, 0xC8, 0x09, 0x8B }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0x63, 0x13 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0xA3, 0x01, 0x00, 0x00, 0x0B, 0x85, 0x00, 0xC8, 0x09, 0xB5 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0x63, 0x13 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0xA3, 0x01, 0x00, 0x00, 0x0B, 0xAD, 0x00, 0xC8, 0x09, 0xDD }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0x63, 0x13 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0xA3, 0x01, 0x00, 0x00, 0x0B, 0xD8, 0x00, 0xC8, 0x09, 0x08 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0x63, 0x13 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0xA3, 0x01, 0x00, 0x00, 0x0C, 0x00, 0x00, 0xC8, 0x09, 0x31 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0x63, 0x13 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0xA3, 0x01, 0x00, 0x00, 0x0C, 0x2A, 0x00, 0xC8, 0x09, 0x5B }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0x63, 0x13 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0xA3, 0x01, 0x00, 0x00, 0x0C, 0x54, 0x00, 0xC8, 0x09, 0x85 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0x63, 0x13 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0xA3, 0x01, 0x00, 0x00, 0x0C, 0x7D, 0x00, 0xC8, 0x09, 0xAE }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0x63, 0x13 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0xA3, 0x01, 0x00, 0x00, 0x0C, 0xA6, 0x00, 0xC8, 0x09, 0xD7 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0x63, 0x13 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0xA3, 0x01, 0x00, 0x00, 0x0C, 0xCF, 0x00, 0xC8, 0x09, 0x00 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0x63, 0x13 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0xA3, 0x01, 0x00, 0x00, 0x0C, 0xF8, 0x00, 0xC8, 0x09, 0x29 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0x63, 0x13 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0xA3, 0x01, 0x00, 0x00, 0x0D, 0x22, 0x00, 0xC8, 0x09, 0x54 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0x63, 0x13 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0xA3, 0x01, 0x00, 0x00, 0x0D, 0x4B, 0x00, 0xC8, 0x09, 0x7D }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0x63, 0x13 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0xA3, 0x01, 0x00, 0x00, 0x0D, 0x73, 0x00, 0xC8, 0x09, 0xA5 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0x63, 0x13 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0xA3, 0x01, 0x00, 0x00, 0x0D, 0x9C, 0x00, 0xC8, 0x09, 0xCE }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0x63, 0x13 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0xA3, 0x01, 0x00, 0x00, 0x0D, 0xC6, 0x00, 0xC8, 0x09, 0xF8 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0x63, 0x13 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0xA3, 0x01, 0x00, 0x00, 0x0D, 0xF0, 0x00, 0xC8, 0x09, 0x22 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0x63, 0x13 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0xA3, 0x01, 0x00, 0x00, 0x0E, 0x19, 0x00, 0xC8, 0x09, 0x4C }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0x63, 0x13 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0xA3, 0x01, 0x00, 0x00, 0x0E, 0x43, 0x00, 0xC8, 0x09, 0x76 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0x63, 0x13 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0xA3, 0x01, 0x00, 0x00, 0x0E, 0x6C, 0x00, 0xC8, 0x09, 0x9F }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0x63, 0x13 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0xA3, 0x01, 0x00, 0x00, 0x0E, 0x96, 0x00, 0xC8, 0x09, 0xC9 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0x63, 0x13 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0xA3, 0x01, 0x00, 0x00, 0x0E, 0xBF, 0x00, 0xC8, 0x09, 0xF2 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0x63, 0x13 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0xA3, 0x01, 0x00, 0x00, 0x0E, 0xE8, 0x00, 0xC8, 0x09, 0x1B }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0x63, 0x13 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0xA3, 0x01, 0x00, 0x00, 0x0F, 0x11, 0x00, 0xC8, 0x09, 0x45 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0x63, 0x13 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0xA3, 0x01, 0x00, 0x00, 0x0F, 0x3A, 0x00, 0xC8, 0x09, 0x6E }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0x63, 0x13 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0xA3, 0x01, 0x00, 0x00, 0x0F, 0x63, 0x00, 0xC8, 0x09, 0x97 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0x63, 0x13 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0xA3, 0x01, 0x00, 0x00, 0x0F, 0x8D, 0x00, 0xC8, 0x09, 0xC1 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0x63, 0x13 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0xA3, 0x01, 0x00, 0x00, 0x0F, 0xB5, 0x00, 0xC8, 0x09, 0xE9 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0x63, 0x13 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0xA3, 0x01, 0x00, 0x00, 0x0F, 0xDF, 0x00, 0xC8, 0x09, 0x13 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0x63, 0x13 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0xA3, 0x01, 0x00, 0x00, 0x10, 0x0A, 0x00, 0xC8, 0x09, 0x3F }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0x63, 0x13 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0xA3, 0x01, 0x00, 0x00, 0x10, 0x31, 0x00, 0x80, 0x09, 0x1E }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0x63, 0x13 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0xA3, 0x01, 0x00, 0x00, 0x10, 0x3D, 0x00, 0x14, 0x09, 0xBE }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0x63, 0x13 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0xA3, 0x01, 0x00, 0x00, 0x10, 0x40, 0x00, 0x00, 0x09, 0xAD }; raw_list.Add(tmparray);

            pair = new SimAckPair();
            pair.request_type = SimAckPair.REQUEST_TYPE.READ_MOTOR_OPEN;
            pair.request = raw_list[0];
            for (int i = 1; i < raw_list.Count; i++)
            {
                simrsp = new SimResponse();
                simrsp.response = raw_list[i];
                simrsp.angle = open_angles[(i + 1) / 2 - 1];
                pair.responses.Add(simrsp);
                i++;
            }
            simulator_data.Add(pair);

            //  Close cover, set parameters (current angle 525, speed 200, acceleration 10)
            pair = new SimAckPair();
            tmparray = new byte[] { 0xB0, 0x62, 0xFF, 0xFF, 0xEF, 0xC0, 0x00, 0xC8, 0x0A, 0x91 };
            pair.request_type = SimAckPair.REQUEST_TYPE.SET_CLOSE_PARAM;
            pair.request = tmparray;
            tmparray = new byte[] { 0xB0, 0x00, 0x00 }; raw_list.Add(tmparray);
            simrsp = new SimResponse();
            simrsp.response = tmparray;
            pair.responses.Add(simrsp);
            simulator_data.Add(pair);

            //  Close cover, read step motor control data
            raw_list.Clear();
            tmparray = new byte[] { 0xB0, 0x63, 0x13 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0xA3, 0x01, 0xFF, 0xFF, 0xFF, 0xFF, 0x00, 0x09, 0x09, 0x62 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0x63, 0x13 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0xA3, 0x01, 0xFF, 0xFF, 0xFF, 0xFB, 0x00, 0x2D, 0x09, 0x82 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0x63, 0x13 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0xA3, 0x01, 0xFF, 0xFF, 0xFF, 0xE6, 0x00, 0xC8, 0x09, 0x08 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0x63, 0x13 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0xA3, 0x01, 0xFF, 0xFF, 0xFF, 0xBD, 0x00, 0xC8, 0x09, 0xDF }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0x63, 0x13 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0xA3, 0x01, 0xFF, 0xFF, 0xFF, 0x93, 0x00, 0xC8, 0x09, 0xB5 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0x63, 0x13 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0xA3, 0x01, 0xFF, 0xFF, 0xFF, 0x6A, 0x00, 0xC8, 0x09, 0x8C }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0x63, 0x13 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0xA3, 0x01, 0xFF, 0xFF, 0xFF, 0x40, 0x00, 0xC8, 0x09, 0x62 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0x63, 0x13 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0xA3, 0x01, 0xFF, 0xFF, 0xFF, 0x18, 0x00, 0xC8, 0x09, 0x3A }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0x63, 0x13 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0xA3, 0x01, 0xFF, 0xFF, 0xFE, 0xEF, 0x00, 0xC8, 0x09, 0x10 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0x63, 0x13 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0xA3, 0x01, 0xFF, 0xFF, 0xFE, 0xC6, 0x00, 0xC8, 0x09, 0xE7 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0x63, 0x13 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0xA3, 0x01, 0xFF, 0xFF, 0xFE, 0x9C, 0x00, 0xC8, 0x09, 0xBD }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0x63, 0x13 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0xA3, 0x01, 0xFF, 0xFF, 0xFE, 0x72, 0x00, 0xC8, 0x09, 0x93 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0x63, 0x13 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0xA3, 0x01, 0xFF, 0xFF, 0xFE, 0x48, 0x00, 0xC8, 0x09, 0x69 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0x63, 0x13 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0xA3, 0x01, 0xFF, 0xFF, 0xFE, 0x1F, 0x00, 0xC8, 0x09, 0x40 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0x63, 0x13 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0xA3, 0x01, 0xFF, 0xFF, 0xFD, 0xF7, 0x00, 0xC8, 0x09, 0x17 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0x63, 0x13 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0xA3, 0x01, 0xFF, 0xFF, 0xFD, 0xCD, 0x00, 0xC8, 0x09, 0xED }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0x63, 0x13 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0xA3, 0x01, 0xFF, 0xFF, 0xFD, 0xA2, 0x00, 0xC8, 0x09, 0xC2 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0x63, 0x13 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0xA3, 0x01, 0xFF, 0xFF, 0xFD, 0x78, 0x00, 0xC8, 0x09, 0x98 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0x63, 0x13 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0xA3, 0x01, 0xFF, 0xFF, 0xFD, 0x4E, 0x00, 0xC8, 0x09, 0x6E }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0x63, 0x13 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0xA3, 0x01, 0xFF, 0xFF, 0xFD, 0x24, 0x00, 0xC8, 0x09, 0x44 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0x63, 0x13 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0xA3, 0x01, 0xFF, 0xFF, 0xFC, 0xFB, 0x00, 0xC8, 0x09, 0x1A }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0x63, 0x13 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0xA3, 0x01, 0xFF, 0xFF, 0xFC, 0xD2, 0x00, 0xC8, 0x09, 0xF1 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0x63, 0x13 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0xA3, 0x01, 0xFF, 0xFF, 0xFC, 0xAA, 0x00, 0xC8, 0x09, 0xC9 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0x63, 0x13 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0xA3, 0x01, 0xFF, 0xFF, 0xFC, 0x7F, 0x00, 0xC8, 0x09, 0x9E }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0x63, 0x13 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0xA3, 0x01, 0xFF, 0xFF, 0xFC, 0x57, 0x00, 0xC8, 0x09, 0x76 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0x63, 0x13 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0xA3, 0x01, 0xFF, 0xFF, 0xFC, 0x2D, 0x00, 0xC8, 0x09, 0x4C }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0x63, 0x13 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0xA3, 0x01, 0xFF, 0xFF, 0xFC, 0x04, 0x00, 0xC8, 0x09, 0x23 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0x63, 0x13 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0xA3, 0x01, 0xFF, 0xFF, 0xFB, 0xDA, 0x00, 0xC8, 0x09, 0xF8 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0x63, 0x13 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0xA3, 0x01, 0xFF, 0xFF, 0xFB, 0xB1, 0x00, 0xC8, 0x09, 0xCF }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0x63, 0x13 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0xA3, 0x01, 0xFF, 0xFF, 0xFB, 0x88, 0x00, 0xC8, 0x09, 0xA6 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0x63, 0x13 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0xA3, 0x01, 0xFF, 0xFF, 0xFB, 0x5E, 0x00, 0xC8, 0x09, 0x7C }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0x63, 0x13 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0xA3, 0x01, 0xFF, 0xFF, 0xFB, 0x36, 0x00, 0xC8, 0x09, 0x54 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0x63, 0x13 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0xA3, 0x01, 0xFF, 0xFF, 0xFB, 0x0C, 0x00, 0xC8, 0x09, 0x2A }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0x63, 0x13 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0xA3, 0x01, 0xFF, 0xFF, 0xFA, 0xE3, 0x00, 0xC8, 0x09, 0x00 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0x63, 0x13 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0xA3, 0x01, 0xFF, 0xFF, 0xFA, 0xB9, 0x00, 0xC8, 0x09, 0xD6 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0x63, 0x13 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0xA3, 0x01, 0xFF, 0xFF, 0xFA, 0x90, 0x00, 0xC8, 0x09, 0xAD }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0x63, 0x13 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0xA3, 0x01, 0xFF, 0xFF, 0xFA, 0x67, 0x00, 0xC8, 0x09, 0x84 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0x63, 0x13 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0xA3, 0x01, 0xFF, 0xFF, 0xFA, 0x3F, 0x00, 0xC8, 0x09, 0x5C }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0x63, 0x13 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0xA3, 0x01, 0xFF, 0xFF, 0xFA, 0x15, 0x00, 0xC8, 0x09, 0x32 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0x63, 0x13 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0xA3, 0x01, 0xFF, 0xFF, 0xF9, 0xEB, 0x00, 0xC8, 0x09, 0x07 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0x63, 0x13 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0xA3, 0x01, 0xFF, 0xFF, 0xF9, 0xC3, 0x00, 0xC8, 0x09, 0xDF }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0x63, 0x13 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0xA3, 0x01, 0xFF, 0xFF, 0xF9, 0x9A, 0x00, 0xC8, 0x09, 0xB6 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0x63, 0x13 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0xA3, 0x01, 0xFF, 0xFF, 0xF9, 0x71, 0x00, 0xC8, 0x09, 0x8D }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0x63, 0x13 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0xA3, 0x01, 0xFF, 0xFF, 0xF9, 0x47, 0x00, 0xC8, 0x09, 0x63 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0x63, 0x13 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0xA3, 0x01, 0xFF, 0xFF, 0xF9, 0x1E, 0x00, 0xC8, 0x09, 0x3A }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0x63, 0x13 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0xA3, 0x01, 0xFF, 0xFF, 0xF8, 0xF4, 0x00, 0xC8, 0x09, 0x0F }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0x63, 0x13 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0xA3, 0x01, 0xFF, 0xFF, 0xF8, 0xC9, 0x00, 0xC8, 0x09, 0xE4 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0x63, 0x13 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0xA3, 0x01, 0xFF, 0xFF, 0xF8, 0xA0, 0x00, 0xC8, 0x09, 0xBB }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0x63, 0x13 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0xA3, 0x01, 0xFF, 0xFF, 0xF8, 0x76, 0x00, 0xC8, 0x09, 0x91 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0x63, 0x13 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0xA3, 0x01, 0xFF, 0xFF, 0xF8, 0x4D, 0x00, 0xC8, 0x09, 0x68 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0x63, 0x13 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0xA3, 0x01, 0xFF, 0xFF, 0xF8, 0x23, 0x00, 0xC8, 0x09, 0x3E }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0x63, 0x13 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0xA3, 0x01, 0xFF, 0xFF, 0xF7, 0xF9, 0x00, 0xC8, 0x09, 0x13 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0x63, 0x13 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0xA3, 0x01, 0xFF, 0xFF, 0xF7, 0xD1, 0x00, 0xC8, 0x09, 0xEB }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0x63, 0x13 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0xA3, 0x01, 0xFF, 0xFF, 0xF7, 0xA8, 0x00, 0xC8, 0x09, 0xC2 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0x63, 0x13 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0xA3, 0x01, 0xFF, 0xFF, 0xF7, 0x7F, 0x00, 0xC8, 0x09, 0x99 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0x63, 0x13 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0xA3, 0x01, 0xFF, 0xFF, 0xF7, 0x56, 0x00, 0xC8, 0x09, 0x70 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0x63, 0x13 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0xA3, 0x01, 0xFF, 0xFF, 0xF7, 0x2D, 0x00, 0xC8, 0x09, 0x47 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0x63, 0x13 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0xA3, 0x01, 0xFF, 0xFF, 0xF7, 0x03, 0x00, 0xC8, 0x09, 0x1D }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0x63, 0x13 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0xA3, 0x01, 0xFF, 0xFF, 0xF6, 0xD9, 0x00, 0xC8, 0x09, 0xF2 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0x63, 0x13 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0xA3, 0x01, 0xFF, 0xFF, 0xF6, 0xB0, 0x00, 0xC8, 0x09, 0xC9 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0x63, 0x13 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0xA3, 0x01, 0xFF, 0xFF, 0xF6, 0x87, 0x00, 0xC8, 0x09, 0xA0 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0x63, 0x13 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0xA3, 0x01, 0xFF, 0xFF, 0xF6, 0x5E, 0x00, 0xC8, 0x09, 0x77 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0x63, 0x13 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0xA3, 0x01, 0xFF, 0xFF, 0xF6, 0x36, 0x00, 0xC8, 0x09, 0x4F }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0x63, 0x13 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0xA3, 0x01, 0xFF, 0xFF, 0xF6, 0x0C, 0x00, 0xC8, 0x09, 0x25 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0x63, 0x13 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0xA3, 0x01, 0xFF, 0xFF, 0xF5, 0xE3, 0x00, 0xC8, 0x09, 0xFB }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0x63, 0x13 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0xA3, 0x01, 0xFF, 0xFF, 0xF5, 0xB9, 0x00, 0xC8, 0x09, 0xD1 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0x63, 0x13 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0xA3, 0x01, 0xFF, 0xFF, 0xF5, 0x90, 0x00, 0xC8, 0x09, 0xA8 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0x63, 0x13 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0xA3, 0x01, 0xFF, 0xFF, 0xF5, 0x66, 0x00, 0xC8, 0x09, 0x7E }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0x63, 0x13 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0xA3, 0x01, 0xFF, 0xFF, 0xF5, 0x3D, 0x00, 0xC8, 0x09, 0x55 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0x63, 0x13 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0xA3, 0x01, 0xFF, 0xFF, 0xF5, 0x14, 0x00, 0xC8, 0x09, 0x2C }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0x63, 0x13 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0xA3, 0x01, 0xFF, 0xFF, 0xF4, 0xEA, 0x00, 0xC8, 0x09, 0x01 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0x63, 0x13 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0xA3, 0x01, 0xFF, 0xFF, 0xF4, 0xC1, 0x00, 0xC8, 0x09, 0xD8 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0x63, 0x13 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0xA3, 0x01, 0xFF, 0xFF, 0xF4, 0x98, 0x00, 0xC8, 0x09, 0xAF }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0x63, 0x13 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0xA3, 0x01, 0xFF, 0xFF, 0xF4, 0x6E, 0x00, 0xC8, 0x09, 0x85 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0x63, 0x13 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0xA3, 0x01, 0xFF, 0xFF, 0xF4, 0x45, 0x00, 0xC8, 0x09, 0x5C }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0x63, 0x13 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0xA3, 0x01, 0xFF, 0xFF, 0xF4, 0x1C, 0x00, 0xC8, 0x09, 0x33 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0x63, 0x13 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0xA3, 0x01, 0xFF, 0xFF, 0xF3, 0xF3, 0x00, 0xC8, 0x09, 0x09 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0x63, 0x13 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0xA3, 0x01, 0xFF, 0xFF, 0xF3, 0xCB, 0x00, 0xC8, 0x09, 0xE1 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0x63, 0x13 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0xA3, 0x01, 0xFF, 0xFF, 0xF3, 0xA2, 0x00, 0xC8, 0x09, 0xB8 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0x63, 0x13 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0xA3, 0x01, 0xFF, 0xFF, 0xF3, 0x77, 0x00, 0xC8, 0x09, 0x8D }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0x63, 0x13 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0xA3, 0x01, 0xFF, 0xFF, 0xF3, 0x4E, 0x00, 0xC8, 0x09, 0x64 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0x63, 0x13 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0xA3, 0x01, 0xFF, 0xFF, 0xF3, 0x24, 0x00, 0xC8, 0x09, 0x3A }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0x63, 0x13 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0xA3, 0x01, 0xFF, 0xFF, 0xF2, 0xFB, 0x00, 0xC8, 0x09, 0x10 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0x63, 0x13 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0xA3, 0x01, 0xFF, 0xFF, 0xF2, 0xD1, 0x00, 0xC8, 0x09, 0xE6 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0x63, 0x13 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0xA3, 0x01, 0xFF, 0xFF, 0xF2, 0xA8, 0x00, 0xC8, 0x09, 0xBD }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0x63, 0x13 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0xA3, 0x01, 0xFF, 0xFF, 0xF2, 0x7E, 0x00, 0xC8, 0x09, 0x93 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0x63, 0x13 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0xA3, 0x01, 0xFF, 0xFF, 0xF2, 0x55, 0x00, 0xC8, 0x09, 0x6A }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0x63, 0x13 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0xA3, 0x01, 0xFF, 0xFF, 0xF2, 0x2C, 0x00, 0xC8, 0x09, 0x41 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0x63, 0x13 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0xA3, 0x01, 0xFF, 0xFF, 0xF2, 0x04, 0x00, 0xC8, 0x09, 0x19 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0x63, 0x13 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0xA3, 0x01, 0xFF, 0xFF, 0xF1, 0xDB, 0x00, 0xC8, 0x09, 0xEF }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0x63, 0x13 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0xA3, 0x01, 0xFF, 0xFF, 0xF1, 0xB2, 0x00, 0xC8, 0x09, 0xC6 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0x63, 0x13 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0xA3, 0x01, 0xFF, 0xFF, 0xF1, 0x8A, 0x00, 0xC8, 0x09, 0x9E }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0x63, 0x13 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0xA3, 0x01, 0xFF, 0xFF, 0xF1, 0x61, 0x00, 0xC8, 0x09, 0x75 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0x63, 0x13 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0xA3, 0x01, 0xFF, 0xFF, 0xF1, 0x39, 0x00, 0xC8, 0x09, 0x4D }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0x63, 0x13 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0xA3, 0x01, 0xFF, 0xFF, 0xF1, 0x10, 0x00, 0xC8, 0x09, 0x24 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0x63, 0x13 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0xA3, 0x01, 0xFF, 0xFF, 0xF0, 0xE7, 0x00, 0xC8, 0x09, 0xFA }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0x63, 0x13 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0xA3, 0x01, 0xFF, 0xFF, 0xF0, 0xBE, 0x00, 0xC8, 0x09, 0xD1 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0x63, 0x13 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0xA3, 0x01, 0xFF, 0xFF, 0xF0, 0x95, 0x00, 0xC8, 0x09, 0xA8 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0x63, 0x13 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0xA3, 0x01, 0xFF, 0xFF, 0xF0, 0x6A, 0x00, 0xC8, 0x09, 0x7D }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0x63, 0x13 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0xA3, 0x01, 0xFF, 0xFF, 0xF0, 0x41, 0x00, 0xC8, 0x09, 0x54 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0x63, 0x13 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0xA3, 0x01, 0xFF, 0xFF, 0xF0, 0x18, 0x00, 0xC8, 0x09, 0x2B }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0x63, 0x13 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0xA3, 0x01, 0xFF, 0xFF, 0xEF, 0xF0, 0x00, 0xC8, 0x09, 0x02 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0x63, 0x13 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0xA3, 0x01, 0xFF, 0xFF, 0xEF, 0xCC, 0x00, 0x65, 0x09, 0x7B }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0x63, 0x13 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0xA3, 0x01, 0xFF, 0xFF, 0xEF, 0xC2, 0x00, 0x0B, 0x09, 0x17 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0x63, 0x13 }; raw_list.Add(tmparray);
            tmparray = new byte[] { 0xB0, 0xA3, 0x01, 0xFF, 0xFF, 0xEF, 0xC0, 0x00, 0x00, 0x09, 0x0A }; raw_list.Add(tmparray);

            pair = new SimAckPair();
            pair.request_type = SimAckPair.REQUEST_TYPE.READ_MOTOR_CLOSE;
            pair.request = raw_list[0];
            for (int i = 1; i < raw_list.Count; i++)
            {
                simrsp = new SimResponse();
                simrsp.response = raw_list[i];
                simrsp.angle = close_angles[(i + 1) / 2 - 1];
                pair.responses.Add(simrsp);
                i++;
            }
            simulator_data.Add(pair);
        }

        private void initialize_open_angles()
        {
            open_angles.Clear();
            open_angles.Add(0);
            open_angles.Add(0);
            open_angles.Add(2);
            open_angles.Add(7);
            open_angles.Add(12);
            open_angles.Add(17);
            open_angles.Add(22);
            open_angles.Add(27);
            open_angles.Add(32);
            open_angles.Add(38);
            open_angles.Add(43);
            open_angles.Add(48);
            open_angles.Add(53);
            open_angles.Add(58);
            open_angles.Add(63);
            open_angles.Add(68);
            open_angles.Add(74);
            open_angles.Add(79);
            open_angles.Add(84);
            open_angles.Add(89);
            open_angles.Add(95);
            open_angles.Add(100);
            open_angles.Add(105);
            open_angles.Add(110);
            open_angles.Add(115);
            open_angles.Add(121);
            open_angles.Add(126);
            open_angles.Add(131);
            open_angles.Add(136);
            open_angles.Add(141);
            open_angles.Add(147);
            open_angles.Add(152);
            open_angles.Add(157);
            open_angles.Add(162);
            open_angles.Add(167);
            open_angles.Add(172);
            open_angles.Add(177);
            open_angles.Add(182);
            open_angles.Add(187);
            open_angles.Add(193);
            open_angles.Add(198);
            open_angles.Add(203);
            open_angles.Add(208);
            open_angles.Add(213);
            open_angles.Add(218);
            open_angles.Add(224);
            open_angles.Add(229);
            open_angles.Add(234);
            open_angles.Add(239);
            open_angles.Add(244);
            open_angles.Add(250);
            open_angles.Add(255);
            open_angles.Add(260);
            open_angles.Add(265);
            open_angles.Add(270);
            open_angles.Add(275);
            open_angles.Add(280);
            open_angles.Add(286);
            open_angles.Add(291);
            open_angles.Add(296);
            open_angles.Add(301);
            open_angles.Add(307);
            open_angles.Add(312);
            open_angles.Add(317);
            open_angles.Add(322);
            open_angles.Add(327);
            open_angles.Add(332);
            open_angles.Add(337);
            open_angles.Add(342);
            open_angles.Add(348);
            open_angles.Add(353);
            open_angles.Add(358);
            open_angles.Add(363);
            open_angles.Add(368);
            open_angles.Add(373);
            open_angles.Add(379);
            open_angles.Add(384);
            open_angles.Add(389);
            open_angles.Add(394);
            open_angles.Add(399);
            open_angles.Add(404);
            open_angles.Add(409);
            open_angles.Add(415);
            open_angles.Add(420);
            open_angles.Add(425);
            open_angles.Add(430);
            open_angles.Add(435);
            open_angles.Add(440);
            open_angles.Add(446);
            open_angles.Add(451);
            open_angles.Add(456);
            open_angles.Add(461);
            open_angles.Add(466);
            open_angles.Add(471);
            open_angles.Add(477);
            open_angles.Add(482);
            open_angles.Add(487);
            open_angles.Add(492);
            open_angles.Add(497);
            open_angles.Add(502);
            open_angles.Add(507);
            open_angles.Add(513);
            open_angles.Add(518);
            open_angles.Add(519);
            open_angles.Add(520);
        }

        private void initialize_close_angles()
        {
            close_angles.Clear();

            close_angles.Add(520);
            close_angles.Add(520);
            close_angles.Add(517);
            close_angles.Add(512);
            close_angles.Add(507);
            close_angles.Add(502);
            close_angles.Add(496);
            close_angles.Add(491);
            close_angles.Add(486);
            close_angles.Add(481);
            close_angles.Add(476);
            close_angles.Add(471);
            close_angles.Add(465);
            close_angles.Add(460);
            close_angles.Add(455);
            close_angles.Add(450);
            close_angles.Add(445);
            close_angles.Add(439);
            close_angles.Add(434);
            close_angles.Add(429);
            close_angles.Add(424);
            close_angles.Add(419);
            close_angles.Add(414);
            close_angles.Add(408);
            close_angles.Add(403);
            close_angles.Add(398);
            close_angles.Add(393);
            close_angles.Add(388);
            close_angles.Add(383);
            close_angles.Add(377);
            close_angles.Add(372);
            close_angles.Add(367);
            close_angles.Add(362);
            close_angles.Add(357);
            close_angles.Add(352);
            close_angles.Add(346);
            close_angles.Add(341);
            close_angles.Add(336);
            close_angles.Add(331);
            close_angles.Add(326);
            close_angles.Add(321);
            close_angles.Add(316);
            close_angles.Add(311);
            close_angles.Add(305);
            close_angles.Add(300);
            close_angles.Add(295);
            close_angles.Add(290);
            close_angles.Add(284);
            close_angles.Add(279);
            close_angles.Add(274);
            close_angles.Add(269);
            close_angles.Add(264);
            close_angles.Add(259);
            close_angles.Add(253);
            close_angles.Add(248);
            close_angles.Add(243);
            close_angles.Add(238);
            close_angles.Add(233);
            close_angles.Add(228);
            close_angles.Add(222);
            close_angles.Add(217);
            close_angles.Add(212);
            close_angles.Add(207);
            close_angles.Add(202);
            close_angles.Add(197);
            close_angles.Add(192);
            close_angles.Add(186);
            close_angles.Add(181);
            close_angles.Add(176);
            close_angles.Add(171);
            close_angles.Add(166);
            close_angles.Add(161);
            close_angles.Add(155);
            close_angles.Add(150);
            close_angles.Add(145);
            close_angles.Add(140);
            close_angles.Add(135);
            close_angles.Add(130);
            close_angles.Add(125);
            close_angles.Add(119);
            close_angles.Add(114);
            close_angles.Add(109);
            close_angles.Add(104);
            close_angles.Add(99);
            close_angles.Add(93);
            close_angles.Add(88);
            close_angles.Add(83);
            close_angles.Add(78);
            close_angles.Add(73);
            close_angles.Add(68);
            close_angles.Add(63);
            close_angles.Add(58);
            close_angles.Add(53);
            close_angles.Add(48);
            close_angles.Add(42);
            close_angles.Add(37);
            close_angles.Add(32);
            close_angles.Add(27);
            close_angles.Add(22);
            close_angles.Add(17);
            close_angles.Add(11);
            close_angles.Add(6);
            close_angles.Add(2);
            close_angles.Add(1);
            close_angles.Add(0);
        }

    }
}


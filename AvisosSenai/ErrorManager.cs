using System;
using System.Device.Gpio;
using System.Threading;

namespace AvisosSenai
{
    public class ErrorManager
    {
        public ErrorStatus Status = ErrorStatus.None;
        private GpioController _gpio;
        private GpioPin _pinRed;
        private GpioPin _pinGreen;

        public ErrorManager(int pinRedNumber, int pinGreenNumber)
        {
            _gpio = new GpioController();

            

            try
            {
               
                _pinRed = _gpio.OpenPin(pinRedNumber, PinMode.Output);
                _pinGreen = _gpio.OpenPin(pinGreenNumber, PinMode.Output);

                new Thread(BlinkLoop).Start();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Erro GPIO: " + ex.Message);
            }
        }

        private void BlinkLoop()
        {
            while (true)
            {
                if (Status == ErrorStatus.None)
                {
                  
                    _pinGreen.Write(PinValue.High);
                    _pinRed.Write(PinValue.Low);
                    Thread.Sleep(500);
                }
                else
                {
                    
                    _pinGreen.Write(PinValue.Low);

                    int timesToBlink = (int)Status;

                  
                    for (int i = 0; i < timesToBlink; i++)
                    {
                        _pinRed.Write(PinValue.High);
                        Thread.Sleep(250);
                        _pinRed.Write(PinValue.Low);
                        Thread.Sleep(250);
                    }

                   
                    Thread.Sleep(3000);
                }
            }
        }
    }
}
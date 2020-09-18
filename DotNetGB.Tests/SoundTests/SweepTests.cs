using System;
using DotNetGB.Hardware.Sounds;
using Xunit;

using static DotNetGB.Gameboy;

namespace DotNetGB.Tests.SoundTests
{
    public class SweepTests
    {
        private readonly FrequencySweep sweep = new FrequencySweep();

        /*
         set_test 2,"If shift>0, calculates on trigger"
         call begin
         wreg NR10,$01
         wreg NR13,$FF
         wreg NR14,$C7
         call should_be_off
         call begin
         wreg NR10,$11
         wreg NR13,$FF
         wreg NR14,$C7
         call should_be_off
         */
        [Fact]
        public void Test04_2()
        {
            Begin();
            WregNR(10, 0x01);
            WregNR(13, 0xff);
            WregNR(14, 0xc7);
            ShouldBeOff();

            Begin();
            WregNR(10, 0x11);
            WregNR(13, 0xff);
            WregNR(14, 0xc7);
            ShouldBeOff();
        }

        /*
         set_test 3,"If shift=0, doesn't calculate on trigger"
         call begin
         wreg NR10,$10
         wreg NR13,$FF
         wreg NR14,$C7
         delay_apu 1
         call should_be_almost_off
         */
        [Fact]
        public void Test04_3()
        {
            Begin();
            WregNR(10, 0x10);
            WregNR(13, 0xff);
            WregNR(14, 0xc7);
            DelayApu(1);
            ShouldBeAlmostOff();
        }

        /*
         set_test 4,"If period=0, doesn't calculate"
         call begin
         wreg NR10,$00
         wreg NR13,$FF
         wreg NR14,$C7
         delay_apu $20
         call should_be_almost_off
         */
        [Fact]
        public void Test04_4()
        {
            Begin();
            WregNR(10, 0x00);
            WregNR(13, 0xff);
            WregNR(14, 0xc7);
            DelayApu(0x20);
            ShouldBeOn();
        }

        /*
         set_test 5,"After updating frequency, calculates a second time"
         call begin
         wreg NR10,$11
         wreg NR13,$00
         wreg NR14,$C5
         delay_apu 1
         call should_be_almost_off
         */
        [Fact]
        public void Test04_5()
        {
            Begin();
            WregNR(10, 0x11);
            WregNR(13, 0x00);
            WregNR(14, 0xc5);
            DelayApu(1);
            ShouldBeAlmostOff();
        }

        /*
         set_test 6,"If calculation>$7FF, disables channel"
         call begin
         wreg NR10,$02
         wreg NR13,$67
         wreg NR14,$C6
         call should_be_off
         */
        [Fact]
        public void Test04_6()
        {
            Begin();
            WregNR(10, 0x02);
            WregNR(13, 0x67);
            WregNR(14, 0xc6);
            ShouldBeOff();
        }

        /*
         set_test 7,"If calculation<=$7FF, doesn't disable channel"
         call begin
         wreg NR10,$01
         wreg NR13,$55
         wreg NR14,$C5
         delay_apu $20
         call should_be_almost_off
         */
        [Fact]
        public void Test04_7()
        {
            Begin();
            WregNR(10, 0x01);
            WregNR(13, 0x55);
            WregNR(14, 0xc5);
            ShouldBeOn();
        }

        /*
         set_test 8,"If shift=0 and period>0, trigger enables"
         call begin
         wreg NR10,$10
         wreg NR13,$FF
         wreg NR14,$C3
         delay_apu 2
         wreg NR10,$11
         delay_apu 1
         call should_be_almost_off
         */
        [Fact]
        public void Test04_8()
        {
            Begin();
            WregNR(10, 0x10);
            WregNR(13, 0xff);
            WregNR(14, 0xc3);
            DelayApu(2);
            WregNR(10, 0x11);
            DelayApu(1);
            ShouldBeAlmostOff();
        }

        /*
         set_test 9,"If shift>0 and period=0, trigger enables"
         call begin
         wreg NR10,$01
         wreg NR13,$FF
         wreg NR14,$C3
         delay_apu 15
         wreg NR10,$11
         call should_be_almost_off
         */
        [Fact]
        public void Test04_9()
        {
            Begin();
            WregNR(10, 0x01);
            WregNR(13, 0xff);
            WregNR(14, 0xc3);
            DelayApu(15);
            WregNR(10, 0x11);
            ShouldBeAlmostOff();
        }

        /*
         set_test 10,"If shift=0 and period=0, trigger disables"
         call begin
         wreg NR10,$08
         wreg NR13,$FF
         wreg NR14,$C3
         wreg NR10,$11
         delay_apu $20
         call should_be_almost_off
         */
        [Fact]
        public void Test04_10()
        {
            Begin();
            WregNR(10, 0x08);
            WregNR(13, 0xff);
            WregNR(14, 0xc3);
            WregNR(10, 0x11);
            DelayApu(0x20);
            ShouldBeOn();
        }

        /*
         set_test 11,"If shift=0, doesn't update"
         call begin
         wreg NR10,$10
         wreg NR13,$FF
         wreg NR14,$C3
         delay_apu $20
         call should_be_almost_off
         */
        [Fact]
        public void Test04_11()
        {
            Begin();
            WregNR(10, 0x10);
            WregNR(13, 0xff);
            WregNR(14, 0xc3);
            DelayApu(0x20);
            ShouldBeOn();
        }

        /*
         set_test 12,"If period=0, doesn't update"
         call begin
         wreg NR10,$01
         wreg NR13,$00
         wreg NR14,$C5
         delay_apu $20
         call should_be_almost_off
         */
        [Fact]
        public void Test04_12()
        {
            Begin();
            WregNR(10, 0x01);
            WregNR(13, 0x00);
            WregNR(14, 0xc5);
            DelayApu(0x20);
            ShouldBeOn();
        }

        /*
         set_test 2,"Timer treats period 0 as 8"
         call begin
         wreg NR10,$11
         wreg NR13,$00
         wreg NR14,$C2
         delay_apu 1
         wreg NR10,$01  ; sweep enabled
         delay_apu 3
         wreg NR10,$11  ; non-zero period so calc will occur when timer reloads
         delay_apu $11
         call should_be_almost_off
         */
        [Fact]
        public void Test05_02()
        {
            Begin();
            WregNR(10, 0x11);
            WregNR(13, 0x00);
            WregNR(14, 0xc2);
            DelayApu(1);
            WregNR(10, 0x01);
            DelayApu(3);
            WregNR(10, 0x11);
            DelayApu(0x11);
            ShouldBeOn();
        }

        /*
        begin:
         call sync_sweep
         wreg NR14,$40
         wreg NR11,-$21
         wreg NR12,$08
         ret
         */
        private void Begin()
        {
            SyncSweep();
            WregNR(14, 0x40);
        }

        private void ShouldBeOn()
        {
            Assert.True(sweep.IsEnabled);
        }

        /*
        should_be_almost_off:
         lda  NR52
         and  $01
         jp   z,test_failed
         delay_apu 1
        should_be_off:
         lda  NR52
         and  $01
         jp   nz,test_failed
         ret
         */
        private void ShouldBeAlmostOff()
        {
            Assert.True(sweep.IsEnabled);
            DelayApu(1);
            ShouldBeOff();
        }

        private void ShouldBeOff()
        {
            Assert.False(sweep.IsEnabled);
        }

        /*
        sync_sweep:
         wreg NR10,$11  ; sweep period = 1, shift = 1
         wreg NR12,$08  ; silent without disabling channel
         wreg NR13,$FF  ; freq = $3FF
         wreg NR14,$83  ; trigger
    -    lda  NR52
         and  $01
         jr   nz,-
         ret
         */
        private void SyncSweep()
        {
            WregNR(10, 0x11);
            WregNR(13, 0xff);
            WregNR(14, 0x83);
            while (sweep.IsEnabled)
            {
                sweep.Tick();
            }
        }

        private void WregNR(int reg, int value)
        {
            switch (reg)
            {
                case 10:
                    sweep.Nr10 = value;
                    break;

                case 13:
                    sweep.Nr13 = value;
                    break;

                case 14:
                    sweep.Nr14 = value;
                    break;

                default:
                    throw new ArgumentException();
            }
        }

        private void DelayApu(int apuCycles)
        {
            for (int i = 0; i < TICKS_PER_SEC / 256 * apuCycles; i++)
            {
                sweep.Tick();
            }
        }
    }
}
using System;

namespace Sylves
{
    public static class StepLengths
    {
        private static readonly Func<Step, float?> uniform = (step) => 1.0f;

        public static Func<Step, float?> Uniform => uniform;

        public static Func<Step, float?> Euclidian(IGrid grid) => (step) => (grid.GetCellCenter(step.Src) - grid.GetCellCenter(step.Dest)).magnitude;

        public static Func<Step, float?> Create(Func<Cell, bool> isAccessible = null, Func<Step, float?> stepLengths = null)
        {
            if(isAccessible == null)
            {
                if(stepLengths == null)
                {
                    return uniform;
                }
                else
                {
                    return stepLengths;
                }
            }
            else
            {
                if(stepLengths == null)
                {
                    return (step) => isAccessible(step.Dest) ? (float?)1 : null;
                }
                else
                {
                    return (step) => isAccessible(step.Dest) ? stepLengths(step) : null;
                }
            }
        }
    }
}

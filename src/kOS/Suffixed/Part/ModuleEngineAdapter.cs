﻿using kOS.Safe.Encapsulation.Part;
using System;
using UnityEngine;

namespace kOS.Suffixed.Part
{
    public class ModuleEngineAdapter : IModuleEngine
    {
        private enum EngineType
        {
            Engine,
            EngineFx
        }

        private readonly ModuleEnginesFX engineModuleFx;
        private readonly ModuleEngines engineModule;
        private readonly EngineType engineType;

        public ModuleEngineAdapter(ModuleEngines engineModule)
        {
            this.engineModule = engineModule;
            engineType = EngineType.Engine;
        }

        public ModuleEngineAdapter(ModuleEnginesFX engineModuleFx)
        {
            this.engineModuleFx = engineModuleFx;
            engineType = EngineType.EngineFx;
        }

        public void Activate()
        {
            switch (engineType)
            {
                case EngineType.Engine:
                    engineModule.Activate();
                    break;

                case EngineType.EngineFx:
                    engineModuleFx.Activate();
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void Shutdown()
        {
            switch (engineType)
            {
                case EngineType.Engine:
                    engineModule.Shutdown();
                    break;

                case EngineType.EngineFx:
                    engineModuleFx.Shutdown();
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public float ThrustPercentage
        {
            get
            {
                switch (engineType)
                {
                    case EngineType.Engine:
                        return engineModule.thrustPercentage;

                    case EngineType.EngineFx:
                        return engineModuleFx.thrustPercentage;

                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            set
            {
                switch (engineType)
                {
                    case EngineType.Engine:
                        engineModule.thrustPercentage = value;
                        break;

                    case EngineType.EngineFx:
                        engineModuleFx.thrustPercentage = value;
                        break;

                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        public float MaxThrust
        {
            get
            {
                switch (engineType)
                {
                    case EngineType.Engine:
                        return (float)GetEngineThrust(engineModule);

                    case EngineType.EngineFx:
                        return (float)GetEngineThrust(engineModuleFx);

                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        public float AvailableThrust
        {
            get
            {
                switch (engineType)
                {
                    case EngineType.Engine:
                        return (float)GetEngineThrust(engineModule, useThrustLimit: true);

                    case EngineType.EngineFx:
                        return (float)GetEngineThrust(engineModuleFx, useThrustLimit: true);

                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        public float FinalThrust
        {
            get
            {
                switch (engineType)
                {
                    case EngineType.Engine:
                        return engineModule.finalThrust;

                    case EngineType.EngineFx:
                        return engineModuleFx.finalThrust;

                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        public static float GetEngineThrust(ModuleEngines engine, float throttle = 1.0f, bool useThrustLimit = false, double atmPressure = -1.0)
        {
            if (engine != null)
            {
                if (!engine.isOperational) return 0.0f;
                if (useThrustLimit) { throttle = throttle * engine.thrustPercentage / 100.0f; }
                if (atmPressure < 0) { atmPressure = engine.part.staticPressureAtm; }
                float flowMod = 1.0f;
                float velMod = 1.0f;
                if (engine.atmChangeFlow)
                {
                    flowMod = (float)(engine.part.atmDensity / 1.225f);
                }
                if (engine.useAtmCurve && engine.atmCurve != null)
                {
                    flowMod = engine.atmCurve.Evaluate(flowMod);
                }
                if (engine.useVelCurve && engine.velCurve != null)
                {
                    velMod = velMod * engine.velCurve.Evaluate((float)engine.vessel.mach);
                }
                // thrust is modified fuel flow rate times isp time g times the velocity modifier for jet engines (as of KSP 1.0)
                return Mathf.Lerp(engine.minFuelFlow, engine.maxFuelFlow, throttle) * flowMod * GetEngineIsp(engine, atmPressure) * engine.g * velMod;
            }
            else return 0.0f;
        }

        public static float GetEngineIsp(ModuleEngines engine)
        {
            if (engine != null)
            {
                return GetEngineIsp(engine, engine.part.staticPressureAtm);
            }
            else return 0.0f;
        }

        public static float GetEngineIsp(ModuleEngines engine, double staticPressureAtm)
        {
            if (engine != null)
            {
                return engine.atmosphereCurve.Evaluate((float)staticPressureAtm);
            }
            else return 0.0f;
        }

        public float FuelFlow
        {
            get
            {
                switch (engineType)
                {
                    case EngineType.Engine:
                        return engineModule.fuelFlowGui;

                    case EngineType.EngineFx:
                        return engineModuleFx.fuelFlowGui;

                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        public float SpecificImpulse
        {
            get
            {
                switch (engineType)
                {
                    case EngineType.Engine:
                        return engineModule.realIsp;

                    case EngineType.EngineFx:
                        return engineModuleFx.realIsp;

                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        public float VacuumSpecificImpluse
        {
            get
            {
                switch (engineType)
                {
                    case EngineType.Engine:
                        return engineModule.atmosphereCurve.Evaluate(0);

                    case EngineType.EngineFx:
                        return engineModuleFx.atmosphereCurve.Evaluate(0);

                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        public float SeaLevelSpecificImpulse
        {
            get
            {
                switch (engineType)
                {
                    case EngineType.Engine:
                        return engineModule.atmosphereCurve.Evaluate(1);

                    case EngineType.EngineFx:
                        return engineModuleFx.atmosphereCurve.Evaluate(1);

                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        public bool Flameout
        {
            get
            {
                switch (engineType)
                {
                    case EngineType.Engine:
                        return engineModule.flameout;

                    case EngineType.EngineFx:
                        return engineModuleFx.flameout;

                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        public bool Ignition
        {
            get
            {
                switch (engineType)
                {
                    case EngineType.Engine:
                        return engineModule.getIgnitionState;

                    case EngineType.EngineFx:
                        return engineModuleFx.getIgnitionState;

                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        public bool AllowRestart
        {
            get
            {
                switch (engineType)
                {
                    case EngineType.Engine:
                        return engineModule.allowRestart;

                    case EngineType.EngineFx:
                        return engineModuleFx.allowRestart;

                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        public bool AllowShutdown
        {
            get
            {
                switch (engineType)
                {
                    case EngineType.Engine:
                        return engineModule.allowShutdown;

                    case EngineType.EngineFx:
                        return engineModuleFx.allowShutdown;

                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        public bool ThrottleLock
        {
            get
            {
                switch (engineType)
                {
                    case EngineType.Engine:
                        return engineModule.throttleLocked;

                    case EngineType.EngineFx:
                        return engineModuleFx.throttleLocked;

                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        public float IspAtAtm(double atmPressure)
        {
            switch (engineType)
            {
                case EngineType.Engine:
                    return ModuleEngineAdapter.GetEngineIsp(engineModule, atmPressure);

                case EngineType.EngineFx:
                    return ModuleEngineAdapter.GetEngineIsp(engineModuleFx, atmPressure);

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public float MaxThrustAtAtm(double atmPressure)
        {
            switch (engineType)
            {
                case EngineType.Engine:
                    return ModuleEngineAdapter.GetEngineThrust(engineModule, atmPressure: atmPressure);

                case EngineType.EngineFx:
                    return ModuleEngineAdapter.GetEngineThrust(engineModuleFx, atmPressure: atmPressure);

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public float AvailableThrustAtAtm(double atmPressure)
        {
            switch (engineType)
            {
                case EngineType.Engine:
                    return ModuleEngineAdapter.GetEngineThrust(engineModule, useThrustLimit: true, atmPressure: atmPressure);

                case EngineType.EngineFx:
                    return ModuleEngineAdapter.GetEngineThrust(engineModuleFx, useThrustLimit: true, atmPressure: atmPressure);

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
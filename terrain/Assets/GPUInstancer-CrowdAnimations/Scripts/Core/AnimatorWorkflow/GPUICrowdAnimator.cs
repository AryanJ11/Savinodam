#if GPU_INSTANCER
using System.Collections.Generic;
using UnityEngine;

namespace GPUInstancer.CrowdAnimations
{
    public class GPUICrowdAnimator : GPUIAnimatorWorkflow
    {
        public Vector4 currentClipStartTimes;
        public GPUIAnimationClipData[] newAnimationClipData;
        public Vector4 newClipStartTimes;
        public float[] currentClipSpeeds;
        
        public GPUICrowdTransition transition;
        public bool isInTransition;

        public GPUICrowdAnimator()
        {
            ResetAnimator();
        }

        #region Public Methods
        public void ResetAnimator()
        {
            currentAnimationClipData = new GPUIAnimationClipData[4];
            currentAnimationClipDataWeights = Vector4.zero;
            currentClipStartTimes = Vector4.zero;
            newAnimationClipData = new GPUIAnimationClipData[4];
            newClipStartTimes = Vector4.zero;
            currentClipSpeeds = new float[4];
            isInTransition = false;
        }

        public void StartAnimation(GPUICrowdRuntimeData runtimeData, int arrayIndex, AnimationClip animationClip, float startTime = -1.0f, float speed = 1.0f, float transitionTime = 0)
        {
            int animationKey = animationClip.GetHashCode();
            if (!runtimeData.animationClipDataDict.ContainsKey(animationKey))
            {
                Debug.LogError("Animation clip was not baked. Can not start animation: " + animationClip.name);
                return;
            }
            GPUIAnimationClipData clipData = runtimeData.animationClipDataDict[animationKey];

            bool hasTransition = transitionTime > 0;

            int previousClipIndex = GetClipIndex(clipData);

            if (speed <= 0)
                speed = 0.000001f;

            if (startTime >= 0)
                newClipStartTimes.x = Time.time - startTime;
            else if (previousClipIndex >= 0)
            {
                if (speed == currentClipSpeeds[previousClipIndex])
                    newClipStartTimes.x = currentClipStartTimes[previousClipIndex];
                else
                    newClipStartTimes.x = GetSpeedRelativeStartTime(Time.time, currentClipStartTimes[previousClipIndex], currentClipSpeeds[previousClipIndex], clipData.length, speed);
            }
            else
                newClipStartTimes.x = Time.time;
            newClipStartTimes.y = 0;
            newClipStartTimes.z = 0;
            newClipStartTimes.w = 0;

            if (hasTransition)
            {
                currentAnimationClipData[3] = currentAnimationClipData[2];
                currentAnimationClipData[2] = currentAnimationClipData[1];
                currentAnimationClipData[1] = currentAnimationClipData[0];
                currentAnimationClipData[0] = clipData;

                currentAnimationClipDataWeights.w = currentAnimationClipDataWeights.z;
                currentAnimationClipDataWeights.z = currentAnimationClipDataWeights.y;
                currentAnimationClipDataWeights.y = currentAnimationClipDataWeights.x;
                currentAnimationClipDataWeights.x = 0.01f;

                currentClipSpeeds[3] = currentClipSpeeds[2];
                currentClipSpeeds[2] = currentClipSpeeds[1];
                currentClipSpeeds[1] = currentClipSpeeds[0];
                currentClipSpeeds[0] = speed;

                newClipStartTimes[3] = currentClipStartTimes[2];
                newClipStartTimes[2] = currentClipStartTimes[1];
                newClipStartTimes[1] = currentClipStartTimes[0];

                if (transition == null)
                    transition = new GPUICrowdTransition();
                transition.SetData(arrayIndex, Time.time, transitionTime, activeClipCount, currentAnimationClipDataWeights, new Vector4(1, 0, 0, 0), 1);
                isInTransition = true;
                if (!runtimeData.transitioningAnimators.Contains(this))
                    runtimeData.transitioningAnimators.Add(this);

                activeClipCount += 1;
                if (activeClipCount > 4)
                    activeClipCount = 4;
            }
            else
            {
                currentAnimationClipData[3] = null;
                currentAnimationClipData[2] = null;
                currentAnimationClipData[1] = null;
                currentAnimationClipData[0] = clipData;

                currentAnimationClipDataWeights.w = 0;
                currentAnimationClipDataWeights.z = 0;
                currentAnimationClipDataWeights.y = 0;
                currentAnimationClipDataWeights.x = 1;

                currentClipSpeeds[0] = speed;

                activeClipCount = 1;
            }


            BlendAnimations(runtimeData, arrayIndex, hasTransition);
        }

        public void StartBlend(GPUICrowdRuntimeData runtimeData, int arrayIndex, Vector4 animationWeights,
            AnimationClip animationClip1, AnimationClip animationClip2, AnimationClip animationClip3 = null, AnimationClip animationClip4 = null,
            float[] animationTimes = null, float[] animationSpeeds = null, float transitionTime = 0)
        {
            float currentTime = Time.time;
            bool hasTransition = transitionTime > 0;
            int previousClipCount = activeClipCount;
            activeClipCount = 2;
            if (animationClip3 != null)
                activeClipCount++;
            if (animationClip4 != null)
                activeClipCount++;
            if (activeClipCount == 4)
                hasTransition = false;
            if (hasTransition)
            {
                if (previousClipCount + activeClipCount > 4)
                    previousClipCount -= (previousClipCount + activeClipCount) % 4;
                activeClipCount += previousClipCount;
            }

            // TODO reorder clips
            int animationKey;
            if (animationClip4 != null)
            {
                animationKey = animationClip4.GetHashCode();
                if (!runtimeData.animationClipDataDict.ContainsKey(animationKey))
                {
                    Debug.LogError("Animation clip was not baked. Can not start animation: " + animationClip4.name);
                    return;
                }
                newAnimationClipData[3] = runtimeData.animationClipDataDict[animationKey];
            }
            else if (hasTransition && activeClipCount == 4)
                newAnimationClipData[3] = currentAnimationClipData[previousClipCount == 1 ? 0 : 1];
            else
                newAnimationClipData[3] = null;


            if (animationClip3 != null)
            {
                animationKey = animationClip3.GetHashCode();
                if (!runtimeData.animationClipDataDict.ContainsKey(animationKey))
                {
                    Debug.LogError("Animation clip was not baked. Can not start animation: " + animationClip3.name);
                    return;
                }
                newAnimationClipData[2] = runtimeData.animationClipDataDict[animationKey];
            }
            else if (hasTransition)
                newAnimationClipData[2] = currentAnimationClipData[0];
            else
                newAnimationClipData[2] = null;

            animationKey = animationClip1.GetHashCode();
            if (!runtimeData.animationClipDataDict.ContainsKey(animationKey))
            {
                Debug.LogError("Animation clip was not baked. Can not start animation: " + animationClip1.name);
                return;
            }
            newAnimationClipData[0] = runtimeData.animationClipDataDict[animationKey];
            animationKey = animationClip2.GetHashCode();
            if (!runtimeData.animationClipDataDict.ContainsKey(animationKey))
            {
                Debug.LogError("Animation clip was not baked. Can not start animation: " + animationClip2.name);
                return;
            }
            newAnimationClipData[1] = runtimeData.animationClipDataDict[animationKey];
            
            for (int i = 0; i < 4; i++)
            {
                if (animationTimes != null && animationTimes.Length > i)
                    newClipStartTimes[i] = animationTimes[i];
                else
                    newClipStartTimes[i] = GetClipTime(newAnimationClipData[i]);

                // Updated with speeds below
            }

            for (int i = 0; i < 4; i++)
            {
                currentAnimationClipData[i] = newAnimationClipData[i];
            }

            if (hasTransition)
            {
                Vector4 newWeights = Vector4.zero;
                for (int i = 0; i < activeClipCount; i++)
                {
                    if (i < activeClipCount - previousClipCount)
                        newWeights[i] = 0.01f;
                    else
                        newWeights[i] = currentAnimationClipDataWeights[i - activeClipCount + previousClipCount];
                }
                currentAnimationClipDataWeights = newWeights;

                if (transition == null)
                    transition = new GPUICrowdTransition();
                transition.SetData(arrayIndex, currentTime, transitionTime, previousClipCount, newWeights, animationWeights, activeClipCount - previousClipCount);
                isInTransition = true;
                if (!runtimeData.transitioningAnimators.Contains(this))
                    runtimeData.transitioningAnimators.Add(this);
            }
            else
                currentAnimationClipDataWeights = animationWeights;

            int speedCount = 0;
            if (animationSpeeds != null)
                speedCount = animationSpeeds.Length;
            for (int i = 0; i < 4; i++)
            {
                if (i < speedCount)
                {
                    if (animationSpeeds[i] <= 0)
                        currentClipSpeeds[i] = 0.000001f;
                    else
                        currentClipSpeeds[i] = animationSpeeds[i];
                }
                else
                    currentClipSpeeds[i] = 1;
                newClipStartTimes[i] = currentTime - (newClipStartTimes[i] / currentClipSpeeds[i]);
            }

            BlendAnimations(runtimeData, arrayIndex, hasTransition);
        }

        public void SetAnimationSpeed(GPUICrowdRuntimeData runtimeData, int arrayIndex, float animationSpeed)
        {
            int crowdAnimIndex = arrayIndex * 4;
            for (int i = 0; i < activeClipCount; i++)
            {
                if (animationSpeed <= 0)
                    animationSpeed = 0.000001f;
                currentClipStartTimes[i] = GetSpeedRelativeStartTime(Time.time, currentClipStartTimes[i], currentClipSpeeds[i], currentAnimationClipData[i].length, animationSpeed);
                runtimeData.crowdAnimatorControllerData[crowdAnimIndex + i].w = currentClipStartTimes[i];
                currentClipSpeeds[i] = animationSpeed;
                runtimeData.crowdAnimatorControllerData[crowdAnimIndex + i].z = animationSpeed;
            }
            runtimeData.crowdAnimatorControllerBuffer.SetData(runtimeData.crowdAnimatorControllerData, crowdAnimIndex, crowdAnimIndex, activeClipCount);
        }

        public void SetAnimationSpeeds(GPUICrowdRuntimeData runtimeData, int arrayIndex, float[] animationSpeeds)
        {
            int speedCount = 0;
            if (animationSpeeds != null)
                speedCount = animationSpeeds.Length;
            int crowdAnimIndex = arrayIndex * 4;
            for (int i = 0; i < activeClipCount; i++)
            {
                if (animationSpeeds[i] <= 0)
                    animationSpeeds[i] = 0.000001f;
                currentClipStartTimes[i] = GetSpeedRelativeStartTime(Time.time, currentClipStartTimes[i], currentClipSpeeds[i], currentAnimationClipData[i].length, animationSpeeds[i]);
                runtimeData.crowdAnimatorControllerData[crowdAnimIndex + i].w = currentClipStartTimes[i];
                if (i < speedCount)
                    currentClipSpeeds[i] = animationSpeeds[i];
                else
                    currentClipSpeeds[i] = 1;
                runtimeData.crowdAnimatorControllerData[crowdAnimIndex + i].z = currentClipSpeeds[i];
            }
            runtimeData.crowdAnimatorControllerBuffer.SetData(runtimeData.crowdAnimatorControllerData, crowdAnimIndex, crowdAnimIndex, activeClipCount);
        }


        public void SetAnimationWeights(GPUICrowdRuntimeData runtimeData, int arrayIndex, Vector4 animationWeights)
        {
            int animationWeightIndex = arrayIndex * 2 + 1;

            // set weights
            currentAnimationClipDataWeights = animationWeights;
            runtimeData.animationData[animationWeightIndex] = currentAnimationClipDataWeights;

            // set data to buffers
            runtimeData.animationDataBuffer.SetData(runtimeData.animationData, animationWeightIndex, animationWeightIndex, 1);
        }
        
        public void ApplyRootMotion(GPUICrowdRuntimeData runtimeData, int arrayIndex, Transform instanceTransform, float currentTime, float lerpAmount)
        {
            Matrix4x4 currentMatrix = runtimeData.instanceDataArray[arrayIndex];
            Vector3 scale = instanceTransform.localScale;
            bool matrixChanged = false;
            for (int i = 0; i < activeClipCount; i++)
            {
                GPUIAnimationClipData clipData = currentAnimationClipData[i];
                if (!clipData.hasRootMotion)
                    continue;
                float clipTotalTime = GetClipTotalTime(i, currentTime);
                if (clipData.isLoopDisabled && clipTotalTime > clipData.length)
                    continue;

                int clipFrame = Mathf.CeilToInt(GetClipFrame(clipTotalTime, clipData.length, clipData.clipFrameCount, clipData.isLoopDisabled));
                GPUIRootMotion motionData = clipData.rootMotion[clipFrame];
                if (!motionData.hasMotion)
                    continue;

                matrixChanged = true;

                float lerp = lerpAmount * currentClipSpeeds[i] * currentAnimationClipDataWeights[i];
                if (motionData.isPositionOnly)
                {
                    Vector3 motionPosition = motionData.motionMatrix.GetColumn(3) * lerp;
                    motionPosition = instanceTransform.rotation * motionPosition;
                    for (int l = 0; l < 3; l++)
                    {
                        currentMatrix[l, 3] += motionPosition[l] * scale[l];
                    }
                }
                else
                {
                    Matrix4x4 motionMatrix = runtimeData.instanceDataArray[arrayIndex] * motionData.motionMatrix;

                    for (int l = 0; l < 16; l++)
                    {
                        float a = currentMatrix[l];
                        currentMatrix[l] = a + ((motionMatrix[l] - a) * lerp);
                    }
                }
            }
            if (matrixChanged)
            {
                runtimeData.instanceDataArray[arrayIndex] = currentMatrix;
                instanceTransform.SetPositionAndRotation(currentMatrix.GetColumn(3), currentMatrix.rotation);
            }
        }

        public bool ApplyTransition(GPUICrowdRuntimeData runtimeData)
        {
            if (Time.time > transition.startTime + transition.totalTime)
            {
                activeClipCount = transition.endActiveClipCount;
                isInTransition = false;
                return false;
            }

            int animationWeightIndex = transition.arrayIndex * 2 + 1;

            currentAnimationClipDataWeights = Vector4.Lerp(transition.startWeights, transition.endWeights, (Time.time - transition.startTime) / transition.totalTime);

            // set weights
            runtimeData.animationData[animationWeightIndex] = currentAnimationClipDataWeights;

            // set data to buffers
            runtimeData.animationDataBuffer.SetData(runtimeData.animationData, animationWeightIndex, animationWeightIndex, 1);
            return true;
        }

        public void ApplyAnimationEvents(GPUICrowdRuntimeData runtimeData, GPUICrowdPrefab crowdInstance, float currentTime, float deltaTime)
        {
            for (int i = 0; i < activeClipCount; i++)
            {
                GPUIAnimationClipData clipData = currentAnimationClipData[i];
                List<GPUIAnimationEvent> gpuiAnimationEvents;
                if (runtimeData.eventDict.TryGetValue(clipData, out gpuiAnimationEvents))
                {
                    float clipTotalTime = GetClipTotalTime(i, currentTime);
                    if (clipData.isLoopDisabled && clipTotalTime > clipData.length)
                        continue;

                    int currentClipFrame = Mathf.CeilToInt(GetClipFrame(i, currentTime, clipData.length, clipData.clipFrameCount, clipData.isLoopDisabled));
                    int previousClipTime = Mathf.CeilToInt(GetClipFrame(i, currentTime - deltaTime, clipData.length, clipData.clipFrameCount, clipData.isLoopDisabled));
                    foreach (GPUIAnimationEvent gpuiAnimationEvent in gpuiAnimationEvents)
                    {
                        if (gpuiAnimationEvent.eventFrame <= currentClipFrame && (gpuiAnimationEvent.eventFrame > previousClipTime || previousClipTime > currentClipFrame))
                            gpuiAnimationEvent.Invoke(crowdInstance, gpuiAnimationEvent.floatParam, gpuiAnimationEvent.intParam, gpuiAnimationEvent.stringParam);
                    }
                }
            }
        }

        public float GetClipTime(GPUICrowdRuntimeData runtimeData, AnimationClip animationClip)
        {
            int animationKey = animationClip.GetHashCode();
            if (!runtimeData.animationClipDataDict.ContainsKey(animationKey))
            {
                Debug.LogError("Animation clip was not baked. Can not get time for animation: " + animationClip.name);
                return 0;
            }
            GPUIAnimationClipData clipData = runtimeData.animationClipDataDict[animationKey];

            return GetClipTime(clipData);
        }

        public float GetClipTime(GPUIAnimationClipData clipData)
        {
            if (clipData != null)
            {
                int clipIndex = GetClipIndex(clipData);
                if (clipIndex >= 0)
                {
                    return GetClipTotalTime(clipIndex, Time.time) % clipData.length;
                }
            }
            return 0;
        }


        public float GetClipTotalTime(int clipIndex, float currentTime)
        {
            if (clipIndex >= 0)
            {
                return (currentTime - currentClipStartTimes[clipIndex]) * currentClipSpeeds[clipIndex];
            }
            return 0;
        }

        public float GetClipFrame(int clipIndex, float currentTime, float clipLength, int clipFrameCount, bool isLoopDisabled)
        {
            float clipTotalTime = GetClipTotalTime(clipIndex, currentTime);
            if (isLoopDisabled && clipTotalTime > clipLength)
                return clipFrameCount - 1;
            else
                return ((clipTotalTime / clipLength) % 1.0f) * (clipFrameCount - 1);
        }

        public float GetClipFrame(float clipTotalTime, float clipLength, int clipFrameCount, bool isLoopDisabled)
        {
            if (isLoopDisabled && clipTotalTime > clipLength)
                return clipFrameCount - 1;
            else
                return ((clipTotalTime / clipLength) % 1.0f) * (clipFrameCount - 1);
        }

        public void SetClipTime(GPUICrowdRuntimeData runtimeData, int arrayIndex, AnimationClip animationClip, float time)
        {
            int animationKey = animationClip.GetHashCode();
            if (!runtimeData.animationClipDataDict.ContainsKey(animationKey))
            {
                Debug.LogError("Animation clip was not baked. Can not get time for animation: " + animationClip.name);
                return;
            }
            GPUIAnimationClipData clipData = runtimeData.animationClipDataDict[animationKey];

            for (int i = 0; i < currentAnimationClipData.Length; i++)
            {
                if (currentAnimationClipData[i] == clipData)
                {
                    newClipStartTimes[i] = Time.time - (time / (currentClipSpeeds[i] > 0 ? currentClipSpeeds[i] : 1));
                    BlendAnimations(runtimeData, arrayIndex);
                    return;
                }
            }
        }

        public void UpdateIndex(GPUICrowdRuntimeData runtimeData, int arrayIndex)
        {
            newClipStartTimes = currentClipStartTimes;
            BlendAnimations(runtimeData, arrayIndex, isInTransition);
        }
        #endregion Public Methods

        #region Private Methods
        private void BlendAnimations(GPUICrowdRuntimeData runtimeData, int arrayIndex, bool hasTransition = false)
        {
            if (!hasTransition && isInTransition)
            {
                runtimeData.transitioningAnimators.Remove(this);
                isInTransition = false;
            }

            int animationIndex = arrayIndex * 2;
            int crowdAnimIndex = arrayIndex * 4;
            currentClipStartTimes = newClipStartTimes;

            GPUIAnimationClipData clipData;
            for (int i = 0; i < 4; i++)
            {
                if (i < activeClipCount)
                {
                    clipData = currentAnimationClipData[i];

                    // set min-max frames and speed
                    runtimeData.crowdAnimatorControllerData[crowdAnimIndex + i].x = clipData.clipStartFrame;
                    // If loop is disabled send max frame as negative (trick to conserve memory)
                    runtimeData.crowdAnimatorControllerData[crowdAnimIndex + i].y = clipData.isLoopDisabled ?
                        -(clipData.clipStartFrame + clipData.clipFrameCount - 1) :
                        clipData.clipStartFrame + clipData.clipFrameCount - 1;
                    runtimeData.crowdAnimatorControllerData[crowdAnimIndex + i].z = currentClipSpeeds[i];
                    runtimeData.crowdAnimatorControllerData[crowdAnimIndex + i].w = newClipStartTimes[i];

                    // set current clip frame
                    runtimeData.animationData[animationIndex][i] = clipData.clipStartFrame + GetClipFrame(i, Time.time, clipData.length, clipData.clipFrameCount, clipData.isLoopDisabled);
                }
                else
                {
                    runtimeData.animationData[animationIndex][i] = -1.0f;
                }
            }
            // set weights
            runtimeData.animationData[animationIndex + 1] = currentAnimationClipDataWeights;

            // set data to buffers
            runtimeData.animationDataBuffer.SetData(runtimeData.animationData, animationIndex, animationIndex, 2);
            runtimeData.crowdAnimatorControllerBuffer.SetData(runtimeData.crowdAnimatorControllerData, crowdAnimIndex, crowdAnimIndex, activeClipCount);

            newClipStartTimes = Vector4.zero;
            runtimeData.disableFrameLerp = true;
        }

        private float GetSpeedRelativeStartTime(float currentTime, float previousStartTime, float previousSpeed, float clipLength, float newSpeed)
        {
            return currentTime - (((currentTime - previousStartTime) * previousSpeed) % clipLength) / newSpeed;
        }

        private int GetClipIndex(GPUIAnimationClipData clipData)
        {
            if (clipData == null)
                return 0;
            for (int i = 0; i < currentAnimationClipData.Length; i++)
            {
                if (currentAnimationClipData[i] != null && currentAnimationClipData[i].animationClip == clipData.animationClip)
                    return i;
            }
            return -1;
        }

        private float GetClipStartTime(GPUIAnimationClipData clipData)
        {
            if (clipData == null)
                return 0;
            for (int i = 0; i < currentAnimationClipData.Length; i++)
            {
                if (currentAnimationClipData[i] == clipData)
                    return currentClipStartTimes[i];
            }
            return Time.time;
        }
        #endregion Private Methods
    }
}
#endif //GPU_INSTANCER
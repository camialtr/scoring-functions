using Newtonsoft.Json;
using MoveSpaceWrapper;
using Microsoft.AspNetCore.Mvc;
using System.Runtime.InteropServices;

namespace fusion_api
{
    [ApiController]
    [Route("[controller]")]
    public unsafe class ScoringController : Controller
    {
        [HttpGet]
        public string Get() => "Scoring API is working!";

        [HttpPost]
        public string ComputeMove()
        {
            try
            {
                ScoreRequest request = JsonConvert.DeserializeObject<ScoreRequest>(new StreamReader(Request.Body).ReadToEndAsync().Result);
                Console.Write(request);
                ScoreManager scoreManager = new();
                scoreManager.Init(true, 60f);
                if (request.moveFileData is null) return "Unable to process MoveSpace!";
                MoveFile file = GetMoveFileFromByteArray(request.moveFileData);
                scoreManager.StartMoveAnalysis((void*)file.data, file.length, request.move.duration);
                List<RecordedAccData> samples = request.recordedAccData;
                for (int sID = 0; sID < samples.Count; ++sID)
                {
                    RecordedAccData sample = samples[sID];
                    if (sID == 0) continue;
                    float prevRatio = sID == 0 ? 0.0f : (samples[sID - 1].mapTime - request.move.time) / request.move.duration;
                    float currentRatio = (sample.mapTime - request.move.time) / request.move.duration;
                    float step = (currentRatio - prevRatio) / sID;
                    for (int i = 0; i < sID; ++i)
                    {
                        float ratio = Clamp(currentRatio - (step * (sID - (i + 1))), 0.0f, 1.0f);
                        scoreManager.bUpdateFromProgressRatioAndAccels(ratio, Clamp(sample.accX, -3.4f, 3.4f), Clamp(sample.accY, -3.4f, 3.4f), Clamp(sample.accZ, -3.4f, 3.4f));
                    }
                }
                scoreManager.StopMoveAnalysis();
                float scoreEnergy = scoreManager.GetLastMoveEnergyAmount();
                float scorePercentage = scoreManager.GetLastMovePercentageScore();
                Marshal.FreeHGlobal(file.data);
                return JsonConvert.SerializeObject(new Scoring() { energy = scoreEnergy, percentage = scorePercentage });
            }
            catch (Exception)
            {
                Response.StatusCode = 400;
                return "Unable to process MoveSpace!";
            }            
        }

        static MoveFile GetMoveFileFromByteArray(byte[] data)
        {
            MoveFile file = new()
            {
                data = Marshal.AllocHGlobal(data.Length),
                length = (uint)data.Length
            };
            Marshal.Copy(data, 0, file.data, data.Length);
            return file;
        }

        static float Clamp(float value, float min, float max)
        {
            float toReturn = value;
            if (value <= min) toReturn = min;
            if (value >= max) toReturn = max;
            return toReturn;
        }
    }
}

namespace fusion_api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            var app = builder.Build();
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }
            app.UseHttpsRedirection();
            app.UseAuthorization();
            app.MapControllers();
            app.Run();
        }
    }

    public struct RecordedAccData
    {
        public int coachID;
        public float accX;
        public float accY;
        public float accZ;
        public float mapTime;
    }

    public struct ScoreRequest
    {
        public Move move;
        public byte[] moveFileData;
        public List<RecordedAccData> recordedAccData;
    }

    public struct MoveFile
    {
        public IntPtr data;
        public uint length;
    }

    public struct Move
    {
        public float time;
        public float duration;
        public string name;
        public int goldMove;
        public int coachID;
    }
}

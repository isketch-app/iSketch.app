namespace iSketch.app.Classes.PassHash;

public class PassHashResult
{
    public byte[] Salt;
    public byte[] Hash;
}
public class PassHashRequest
{
    public byte[] Salt;
    public string Pass;
}
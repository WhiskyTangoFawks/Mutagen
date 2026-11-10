using System.Text;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Binary.Headers;
using Mutagen.Bethesda.Plugins.Binary.Streams;
using Mutagen.Bethesda.Plugins.Masters;
using Mutagen.Bethesda.Plugins.Meta;

namespace Mutagen.Bethesda.UnitTests.Plugins.Records;

internal static class RawRecordBytes
{
    public static readonly ModKey TestModKey = new("Test", ModType.Plugin);

    public static ParsingMeta Meta(GameConstants constants)
    {
        var masters = SeparatedMasterPackage.NotSeparate(new MasterReferenceCollection(TestModKey));
        return new ParsingMeta(constants, TestModKey, masters);
    }

    public static byte[] Str(string s) => Encoding.ASCII.GetBytes(s + "\0");

    public static byte[] U32(uint value) => BitConverter.GetBytes(value);

    public static byte[] Record(GameConstants constants, string type, params (string Type, byte[] Content)[] subrecords)
    {
        var content = new MemoryStream();
        foreach (var (subType, subContent) in subrecords)
        {
            content.Write(Encoding.ASCII.GetBytes(subType));
            content.Write(BitConverter.GetBytes(checked((ushort)subContent.Length)));
            content.Write(subContent);
        }
        var body = content.ToArray();

        var record = new MemoryStream();
        record.Write(Encoding.ASCII.GetBytes(type));
        record.Write(BitConverter.GetBytes(body.Length));
        record.Write(BitConverter.GetBytes(0));
        record.Write(BitConverter.GetBytes(0x800));
        record.Write(BitConverter.GetBytes(0));
        if (constants.MajorConstants.HeaderLength == 24)
        {
            record.Write(BitConverter.GetBytes((ushort)(constants.DefaultFormVersion ?? 0)));
            record.Write(BitConverter.GetBytes((ushort)0));
        }
        record.Write(body);
        return record.ToArray();
    }

    public static byte[] Group(GameConstants constants, byte[] label, int groupType, params byte[][] contents)
    {
        var body = contents.SelectMany(x => x).ToArray();
        var group = new MemoryStream();
        group.Write(Encoding.ASCII.GetBytes("GRUP"));
        group.Write(BitConverter.GetBytes(constants.GroupConstants.HeaderLength + body.Length));
        group.Write(label);
        group.Write(BitConverter.GetBytes(groupType));
        group.Write(new byte[constants.GroupConstants.HeaderLength - 16]);
        group.Write(body);
        return group.ToArray();
    }

    public static byte[] Plugin(GameConstants constants, params byte[][] topGroups)
    {
        var hedr = BitConverter.GetBytes(1.0f)
            .Concat(BitConverter.GetBytes(1))
            .Concat(BitConverter.GetBytes(0x801))
            .ToArray();
        var header = Record(constants, "TES4", ("HEDR", hedr));
        BitConverter.GetBytes(0).CopyTo(header, 12);
        return header.Concat(topGroups.SelectMany(x => x)).ToArray();
    }

    public static List<string> SubrecordTypes(GameConstants constants, byte[] bytes)
    {
        return constants.MajorRecord(bytes).EnumerateSubrecords().Select(s => s.RecordType.Type).ToList();
    }
}

using System.IO.Abstractions.TestingHelpers;
using System.Text;
using Mutagen.Bethesda.Assets;
using Mutagen.Bethesda.Fallout4;
using Mutagen.Bethesda.Fallout4.Assets;
using Mutagen.Bethesda.Fallout4.Internals;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Assets;
using Mutagen.Bethesda.Plugins.Meta;
using Shouldly;
using Xunit;
using static Mutagen.Bethesda.UnitTests.Plugins.Records.RawRecordBytes;

namespace Mutagen.Bethesda.UnitTests.Plugins.Records.Fallout4;

public class AssetLinkTests
{
    private static readonly GameConstants Constants = GameConstants.Fallout4;

    private static readonly IAssetType Model = Fallout4ModelAssetType.Instance;
    private static readonly IAssetType DeformedModel = Fallout4DeformedModelAssetType.Instance;
    private static readonly IAssetType Behavior = Fallout4BehaviorAssetType.Instance;
    private static readonly IAssetType Texture = Fallout4TextureAssetType.Instance;
    private static readonly IAssetType Material = Fallout4MaterialAssetType.Instance;
    private static readonly IAssetType Sound = Fallout4SoundAssetType.Instance;
    private static readonly IAssetType Music = Fallout4MusicAssetType.Instance;
    private static readonly IAssetType Interface = Fallout4InterfaceAssetType.Instance;
    private static readonly IAssetType Program = Fallout4ProgramAssetType.Instance;

    private record Link(string Type, string File, IAssetType AssetType);

    private record Case((string Type, byte[] Content)[] Subrecords, Link[] Links);

    private static (string, byte[]) Sub(string type, byte[] content) => (type, content);

    private static Case Paths(params Link[] links) => Paths([], links);

    private static Case Paths((string, byte[])[] before, params Link[] links) =>
        new(before.Concat(links.Select(l => Sub(l.Type, Str(l.File)))).ToArray(), links);

    private static byte[] DistantLods(params string[] meshes) =>
        meshes.SelectMany(m => Str(m).Concat(new byte[260 - m.Length - 1])).ToArray();

    private static readonly Dictionary<string, Case> Cases = new()
    {
        ["AMMO"] = Paths(new Link("NAM1", "casing.nif", Model)),
        ["ARMA"] = Paths(
            [Sub("DNAM", new byte[12])],
            new Link("MOD2", "male.nif", Model),
            new Link("MOD3", "female.nif", Model),
            new Link("MOD4", "male1st.nif", Model),
            new Link("MOD5", "female1st.nif", Model)),
        ["BPTD"] = Paths(
            [Sub("BPTN", Str("part")), Sub("BPNN", Str("node")), Sub("BPNT", Str("target"))],
            new Link("NAM1", "limb.nif", Model)),
        ["CELL"] = Paths(new Link("XWEM", "cellenvironment.dds", Texture)),
        ["CLAS"] = Paths(new Link("ICON", "class.dds", Texture)),
        ["CLMT"] = Paths(
            new Link("FNAM", "sun.dds", Texture),
            new Link("GNAM", "sunglare.dds", Texture)),
        ["DEBR"] = new(
            [Sub("DATA", new byte[] { 50 }.Concat(Str("debris.nif")).Concat(new byte[] { 1 }).ToArray())],
            [new Link("DATA", "debris.nif", Model)]),
        ["EFSH"] = Paths(
            new Link("ICON", "fill.dds", Texture),
            new Link("ICO2", "particleshader.dds", Texture),
            new Link("NAM7", "holes.dds", Texture),
            new Link("NAM8", "membranepalette.dds", Texture),
            new Link("NAM9", "particlepalette.dds", Texture)),
        ["FACT"] = Paths(
            [Sub("RNAM", U32(0))],
            new Link("INAM", "insignia.dds", Texture)),
        ["FURN"] = Paths(new Link("XMRK", "furnituremarker.nif", Model)),
        ["HDPT"] = Paths(
            [Sub("NAM0", U32(0))],
            new Link("NAM1", "part.tri", DeformedModel)),
        ["IDLE"] = Paths(
            new Link("DNAM", "behavior.hkx", Behavior),
            new Link("GNAM", "animation.hkx", Behavior)),
        ["IMGS"] = Paths(new Link("TX00", "lut.dds", Texture)),
        ["LENS"] = Paths(
            [Sub("LFSP", U32(1)), Sub("DNAM", Str("sprite"))],
            new Link("FNAM", "sprite.dds", Texture)),
        ["LIGH"] = Paths(new Link("NAM0", "gobo.dds", Texture)),
        ["LSCR"] = Paths(new Link("MOD2", "camera.nif", Model)),
        ["MATT"] = Paths(new Link("ANAM", "breakable.nif", Model)),
        ["MESG"] = Paths(new Link("SNAM", "message.swf", Interface)),
        ["MSWP"] = Paths(
            new Link("BNAM", "original.bgsm", Material),
            new Link("SNAM", "replacement.bgsm", Material)),
        ["MUST"] = Paths(
            new Link("ANAM", "track.wav", Music),
            new Link("BNAM", "finale.wav", Music)),
        ["NOCM"] = Paths(
            [Sub("INDX", U32(0))],
            new Link("NAM1", "obstacle.nif", Model)),
        ["NOTE"] = Paths(
            [Sub("DNAM", new byte[] { (byte)Holotape.Types.Program })],
            new Link("PNAM", "program.swf", Program)),
        ["PERK"] = Paths(
            new Link("ICON", "perk.dds", Texture),
            new Link("FNAM", "perk.swf", Interface)),
        ["PROJ"] = Paths(new Link("NAM1", "muzzleflash.nif", Model)),
        ["QUST"] = Paths(new Link("SNAM", "quest.swf", Interface)),
        ["RACE"] = new(
            [
                Sub("MNAM", []),
                Sub("ANAM", Str("maleskeleton.nif")),
                Sub("FNAM", []),
                Sub("ANAM", Str("femaleskeleton.nif")),
                Sub("NAM0", []),
                Sub("MNAM", []),
                Sub("TTGP", Str("group")),
                Sub("TETI", new byte[4]),
                Sub("TTGP", Str("option")),
                Sub("TTET", Str("tint.dds")),
                Sub("SGNM", Str("subgraph.hkx")),
                Sub("SRAF", new byte[4]),
                Sub("HNAM", Str("haircolor.dds")),
                Sub("HLTX", Str("haircolorextended.dds")),
            ],
            [
                new Link("ANAM", "maleskeleton.nif", Model),
                new Link("ANAM", "femaleskeleton.nif", Model),
                new Link("TTET", "tint.dds", Texture),
                new Link("SGNM", "subgraph.hkx", Behavior),
                new Link("HNAM", "haircolor.dds", Texture),
                new Link("HLTX", "haircolorextended.dds", Texture),
            ]),
        ["SNDR"] = new(
            [
                Sub("CNAM", U32((uint)SoundDescriptorBinaryCreateTranslation.DescriptorType.AutoWeapon)),
                Sub("ANAM", Str("first.wav")),
                Sub("ANAM", Str("second.wav")),
                Sub("ITMC", U32(1)),
                Sub("ITMS", []),
                Sub("INTV", U32(600)),
                Sub("FNAM", Str("rateoffire.wav")),
                Sub("ITME", []),
            ],
            [
                new Link("ANAM", "first.wav", Sound),
                new Link("ANAM", "second.wav", Sound),
                new Link("FNAM", "rateoffire.wav", Sound),
            ]),
        ["SPGD"] = Paths(new Link("MNAM", "particle.bgem", Material)),
        ["STAT"] = new(
            [Sub("MODL", Str("static.nif")), Sub("MNAM", DistantLods("lod0.nif", "lod1.nif", "lod2.nif", "lod3.nif"))],
            [
                new Link("MODL", "static.nif", Model),
                new Link("MNAM", "lod0.nif", Model),
                new Link("MNAM", "lod1.nif", Model),
                new Link("MNAM", "lod2.nif", Model),
                new Link("MNAM", "lod3.nif", Model),
            ]),
        ["TERM"] = Paths(
            [Sub("ISIZ", U32(1)), Sub("ITXT", Str("item")), Sub("ANAM", new byte[] { 16 })],
            new Link("VNAM", "image.dds", Texture),
            new Link("XMRK", "terminalmarker.nif", Model)),
        ["TXST"] = Paths(
            new Link("TX00", "diffuse.dds", Texture),
            new Link("TX01", "normal.dds", Texture),
            new Link("TX02", "wrinkles.dds", Texture),
            new Link("TX03", "glow.dds", Texture),
            new Link("TX04", "height.dds", Texture),
            new Link("TX05", "environment.dds", Texture),
            new Link("TX06", "multilayer.dds", Texture),
            new Link("TX07", "smoothspec.dds", Texture),
            new Link("MNAM", "decal.bgsm", Material)),
        ["WATR"] = Paths(
            [Sub("DNAM", new byte[201])],
            new Link("NAM2", "noise1.dds", Texture),
            new Link("NAM3", "noise2.dds", Texture),
            new Link("NAM4", "noise3.dds", Texture)),
        ["WEAP"] = Paths(
            new Link("MODL", "weapon.nif", Model),
            new Link("ICON", "inventory.dds", Texture),
            new Link("MICO", "message.dds", Texture),
            new Link("MOD4", "weapon1st.nif", Model)),
        ["WRLD"] = Paths(
            new Link("ICON", "map.dds", Texture),
            new Link("NNAM", "canopyshadow.dds", Texture),
            new Link("XWEM", "worldenvironment.dds", Texture),
            new Link("TNAM", "hdloddiffuse.dds", Texture),
            new Link("UNAM", "hdlodnormal.dds", Texture)),
        ["WTHR"] = Paths(
            new Link("00TX", "cloud0.dds", Texture),
            new Link("A0TX", "cloud17.dds", Texture)),
    };

    public static TheoryData<string> RecordTypes => new(Cases.Keys);

    [Theory, MemberData(nameof(RecordTypes))]
    public void EachPathTheRecordNamesIsAnAssetLinkOfItsType(string recordType)
    {
        var testCase = Cases[recordType];
        var fileSystem = new MockFileSystem();
        var path = new ModPath(TestModKey, "/Data/Test.esp");
        fileSystem.Directory.CreateDirectory("/Data");
        fileSystem.File.WriteAllBytes(path, PluginHolding(recordType, testCase));

        ShouldListThePaths(fileSystem, path, testCase);

        var imported = Fallout4Mod.Create(Fallout4Release.Fallout4)
            .FromPath(path)
            .WithFileSystem(fileSystem)
            .Mutable()
            .Construct();
        var rewritePath = new ModPath(TestModKey, "/Rewrite/Test.esp");
        fileSystem.Directory.CreateDirectory("/Rewrite");
        imported.BeginWrite
            .ToPath(rewritePath)
            .WithNoLoadOrder()
            .WithFileSystem(fileSystem)
            .Write();

        ShouldListThePaths(fileSystem, rewritePath, testCase);
    }

    private static byte[] PluginHolding(string recordType, Case testCase)
    {
        var record = Record(Constants, recordType, [Sub("EDID", Str("Test")), ..testCase.Subrecords]);
        var contents = recordType == "CELL"
            ? Group(Constants, U32(0), 2, Group(Constants, U32(0), 3, record))
            : record;
        return Plugin(Constants, Group(Constants, Encoding.ASCII.GetBytes(recordType), 0, contents));
    }

    private static void ShouldListThePaths(MockFileSystem fileSystem, ModPath path, Case testCase)
    {
        using var overlay = Fallout4Mod.Create(Fallout4Release.Fallout4)
            .FromPath(path)
            .WithFileSystem(fileSystem)
            .Construct();
        var imported = Fallout4Mod.Create(Fallout4Release.Fallout4)
            .FromPath(path)
            .WithFileSystem(fileSystem)
            .Mutable()
            .Construct();
        var expected = testCase.Links
            .Select(p => (p.AssetType, Path.Combine(p.AssetType.BaseFolder, p.File)))
            .ToArray();

        foreach (IFallout4ModGetter mod in new IFallout4ModGetter[] { overlay, imported })
        {
            var record = mod.EnumerateMajorRecords().ShouldHaveSingleItem();
            record.ShouldBeAssignableTo<IAssetLinkContainerGetter>()!
                .EnumerateAssetLinks(AssetLinkQuery.Listed)
                .Select(l => (l.AssetTypeInstance, l.DataRelativePath.Path))
                .ShouldBe(expected, ignoreOrder: true);
        }
    }
}

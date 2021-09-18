using System;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using BotNet.Services.Tenor;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using Xunit;

namespace BotNet.Tests.Services.Tenor {
	public class TenorClientTests {
		[Fact]
		public async Task SearchGifsAsync_ShouldReturnGifsAsync() {
			await TestHttpClientUsingDummyContentAsync(
				content: "{\"results\":[{\"id\":\"12795635\",\"title\":\"\",\"h1_title\":\"\",\"media\":[{\"gif\":{\"dims\":[498,256],\"preview\":\"https://media.tenor.com/images/abf8fb7ce9329350fbacdc0ec28de131/tenor.png\",\"url\":\"https://media.tenor.com/images/1d2eb72112f73b9a2b7d775d5dd6e13f/tenor.gif\",\"size\":2259597},\"mp4\":{\"dims\":[598,308],\"preview\":\"https://media.tenor.com/images/abf8fb7ce9329350fbacdc0ec28de131/tenor.png\",\"size\":118748,\"duration\":2.1,\"url\":\"https://media.tenor.com/videos/87beec258e27a1c77beca94deef74436/mp4\"},\"tinygif\":{\"preview\":\"https://media.tenor.com/images/1241de26bb48fda52b3a04dfadba82ef/tenor.gif\",\"dims\":[220,113],\"size\":143364,\"url\":\"https://media.tenor.com/images/4023cafa5d3ae994fa81fa12d09d9dde/tenor.gif\"}}],\"bg_color\":\"\",\"created\":1540979557.901674,\"itemurl\":\"https://tenor.com/view/kaget-ampun-syok-loncat-terkejut-gif-12795635\",\"url\":\"https://tenor.com/1QTn.gif\",\"tags\":[],\"flags\":[],\"shares\":1,\"hasaudio\":true,\"hascaption\":false,\"source_id\":\"\",\"composite\":null},{\"id\":\"10098172\",\"title\":\"saya tidak tahu\",\"h1_title\":\"\",\"media\":[{\"gif\":{\"size\":607964,\"url\":\"https://media.tenor.com/images/37d0e86a4c0824c8a2507267ea7ab0db/tenor.gif\",\"dims\":[256,178],\"preview\":\"https://media.tenor.com/images/8c6a3d3a3ded7df8e050a0b2dca77db9/tenor.png\"},\"tinygif\":{\"preview\":\"https://media.tenor.com/images/4ad0786edf740a1336a592ff0acd04a1/tenor.gif\",\"dims\":[220,153],\"size\":57399,\"url\":\"https://media.tenor.com/images/a8d2e9208eb3f4a8009a477ab3cbbcf9/tenor.gif\"},\"mp4\":{\"url\":\"https://media.tenor.com/videos/338baaf8ea62f20deb87d47efb128624/mp4\",\"preview\":\"https://media.tenor.com/images/8c6a3d3a3ded7df8e050a0b2dca77db9/tenor.png\",\"size\":29326,\"duration\":2,\"dims\":[256,178]}}],\"bg_color\":\"\",\"created\":1509025274.472195,\"itemurl\":\"https://tenor.com/view/jokowi-joko-widodo-tidak-tahu-bingung-confused-gif-10098172\",\"url\":\"https://tenor.com/Qw9U.gif\",\"tags\":[],\"flags\":[],\"shares\":1,\"hasaudio\":false,\"hascaption\":false,\"source_id\":\"\",\"composite\":null},{\"id\":\"19717121\",\"title\":\"\",\"h1_title\":\"\",\"media\":[{\"gif\":{\"url\":\"https://media.tenor.com/images/589b5a619da1bfc8c68a6781cd466400/tenor.gif\",\"dims\":[498,498],\"preview\":\"https://media.tenor.com/images/278b52c3d1fa313ba5f9353020b3d84f/tenor.png\",\"size\":9877128},\"mp4\":{\"dims\":[640,640],\"duration\":5.1,\"url\":\"https://media.tenor.com/videos/c80b49e633ab65edd487778792555deb/mp4\",\"size\":1769588,\"preview\":\"https://media.tenor.com/images/278b52c3d1fa313ba5f9353020b3d84f/tenor.png\"},\"tinygif\":{\"dims\":[220,220],\"size\":1013264,\"preview\":\"https://media.tenor.com/images/f16b6ac57ab67e03aa6bce001d214a71/tenor.gif\",\"url\":\"https://media.tenor.com/images/ede84f27a8b362bdcfa1813f49b069b8/tenor.gif\"}}],\"bg_color\":\"\",\"created\":1608994799.828002,\"itemurl\":\"https://tenor.com/view/jokowi-bodo-amat-gif-19717121\",\"url\":\"https://tenor.com/buTuf.gif\",\"tags\":[],\"flags\":[],\"shares\":1,\"hasaudio\":false,\"hascaption\":false,\"source_id\":\"\",\"composite\":null},{\"id\":\"15199159\",\"title\":\"\",\"h1_title\":\"\",\"media\":[{\"gif\":{\"size\":5687332,\"dims\":[498,498],\"url\":\"https://media.tenor.com/images/215970d9932231e0ce3e642cd577e2af/tenor.gif\",\"preview\":\"https://media.tenor.com/images/d3f5c9308a1d422d14d65ef920d7d27c/tenor.png\"},\"tinygif\":{\"preview\":\"https://media.tenor.com/images/0983ab8087c101d6e14679330ee7256e/tenor.gif\",\"url\":\"https://media.tenor.com/images/0bae84fc5af7cc62c7c5aaf12ec1673f/tenor.gif\",\"dims\":[220,220],\"size\":309736},\"mp4\":{\"duration\":2.6,\"dims\":[640,640],\"preview\":\"https://media.tenor.com/images/d3f5c9308a1d422d14d65ef920d7d27c/tenor.png\",\"size\":378959,\"url\":\"https://media.tenor.com/videos/2ff0cbd84cdab3877ad992e230241b0b/mp4\"}}],\"bg_color\":\"\",\"created\":1570099070.813463,\"itemurl\":\"https://tenor.com/view/jokowi-jae-jok-mukidi-laugh-gif-15199159\",\"url\":\"https://tenor.com/bbV9T.gif\",\"tags\":[],\"flags\":[],\"shares\":1,\"hasaudio\":false,\"hascaption\":false,\"source_id\":\"\",\"composite\":null},{\"id\":\"10079759\",\"title\":\"\",\"h1_title\":\"\",\"media\":[{\"mp4\":{\"url\":\"https://media.tenor.com/videos/955dc79832b7e0a8052292da7c6aa10c/mp4\",\"preview\":\"https://media.tenor.com/images/397028936366915a634d1dc1635c202e/tenor.png\",\"dims\":[374,480],\"size\":373993,\"duration\":11.8},\"gif\":{\"size\":12020630,\"url\":\"https://media.tenor.com/images/b1b679d7f40ba27e23382e02ca126eec/tenor.gif\",\"dims\":[374,480],\"preview\":\"https://media.tenor.com/images/397028936366915a634d1dc1635c202e/tenor.png\"},\"tinygif\":{\"size\":621831,\"preview\":\"https://media.tenor.com/images/ef2464ceeb4ed1cdccd19d356ad89d1d/tenor.gif\",\"dims\":[220,282],\"url\":\"https://media.tenor.com/images/56a04d0d70184f4ea992f2c2f2b38c50/tenor.gif\"}}],\"bg_color\":\"\",\"created\":1508840128.832379,\"itemurl\":\"https://tenor.com/view/jokowi-hormat-siap-oke-joko-widodo-gif-10079759\",\"url\":\"https://tenor.com/QsmV.gif\",\"tags\":[],\"flags\":[],\"shares\":1,\"hasaudio\":true,\"hascaption\":false,\"source_id\":\"\",\"composite\":null},{\"id\":\"21554064\",\"title\":\"\",\"h1_title\":\"\",\"media\":[{\"gif\":{\"size\":11872366,\"dims\":[498,498],\"url\":\"https://media.tenor.com/images/b1c59e85748ff98d5d9d013942c80c0a/tenor.gif\",\"preview\":\"https://media.tenor.com/images/287daa6347af5d5b465c692ad6d289c5/tenor.png\"},\"mp4\":{\"url\":\"https://media.tenor.com/videos/a9e920c770f44b837e326e0f40cbf045/mp4\",\"preview\":\"https://media.tenor.com/images/287daa6347af5d5b465c692ad6d289c5/tenor.png\",\"size\":1689786,\"dims\":[640,640],\"duration\":7.6},\"tinygif\":{\"preview\":\"https://media.tenor.com/images/8e29e924f6e5507b9656a91d92f06d4e/tenor.gif\",\"url\":\"https://media.tenor.com/images/e5d5d9636949adab819f7e442ae5274b/tenor.gif\",\"size\":416571,\"dims\":[220,220]}}],\"bg_color\":\"\",\"created\":1620972675.290043,\"itemurl\":\"https://tenor.com/view/jokowi-bugcat-gif-21554064\",\"url\":\"https://tenor.com/bCBmm.gif\",\"tags\":[],\"flags\":[],\"shares\":1,\"hasaudio\":false,\"hascaption\":false,\"source_id\":\"\",\"composite\":null},{\"id\":\"5963262\",\"title\":\"Presiden Jokowi Optimis\",\"h1_title\":\"\",\"media\":[{\"gif\":{\"url\":\"https://media.tenor.com/images/6090c6322ca095efc63610d8e3bfbcac/tenor.gif\",\"dims\":[256,142],\"preview\":\"https://media.tenor.com/images/43f955cfa6641332cf41a649339a8405/tenor.png\",\"size\":482819},\"tinygif\":{\"url\":\"https://media.tenor.com/images/7d757e27490cd0df7f1e8e1b7905b866/tenor.gif\",\"preview\":\"https://media.tenor.com/images/91acbaa2ee19fbad3a8c20079665cfb7/tenor.gif\",\"size\":192014,\"dims\":[220,122]},\"mp4\":{\"url\":\"https://media.tenor.com/videos/36d530facd77ee05f1bfd14c6891502c/mp4\",\"size\":42948,\"duration\":2.1,\"dims\":[256,142],\"preview\":\"https://media.tenor.com/images/43f955cfa6641332cf41a649339a8405/tenor.png\"}}],\"bg_color\":\"\",\"created\":1473308286.09813,\"itemurl\":\"https://tenor.com/view/jokowi-optimis-naik-roket-lucu-gif-5963262\",\"url\":\"https://tenor.com/zbtO.gif\",\"tags\":[],\"flags\":[],\"shares\":1,\"hasaudio\":false,\"hascaption\":false,\"source_id\":\"\",\"composite\":null},{\"id\":\"22222970\",\"title\":\"\",\"h1_title\":\"\",\"media\":[{\"gif\":{\"preview\":\"https://media.tenor.com/images/1d8daf5b32d1acc0278e4abde8acccaf/tenor.png\",\"dims\":[498,498],\"size\":3351986,\"url\":\"https://media.tenor.com/images/0f648dd724a2aae251e7324e9532d65f/tenor.gif\"},\"mp4\":{\"dims\":[640,640],\"url\":\"https://media.tenor.com/videos/5c96539012fdded15901d72fe717ed61/mp4\",\"preview\":\"https://media.tenor.com/images/1d8daf5b32d1acc0278e4abde8acccaf/tenor.png\",\"duration\":2.7,\"size\":452705},\"tinygif\":{\"preview\":\"https://media.tenor.com/images/957404d1cd879cb50f02f81a1a0aa7f0/tenor.gif\",\"url\":\"https://media.tenor.com/images/2c18b2e7f4dbf02bf74433fe58d7ff12/tenor.gif\",\"size\":243237,\"dims\":[220,220]}}],\"bg_color\":\"\",\"created\":1625459424.575828,\"itemurl\":\"https://tenor.com/view/owi-kun-gif-22222970\",\"url\":\"https://tenor.com/bFpna.gif\",\"tags\":[],\"flags\":[],\"shares\":1,\"hasaudio\":false,\"hascaption\":false,\"source_id\":\"\",\"composite\":null},{\"id\":\"18777951\",\"title\":\"\",\"h1_title\":\"\",\"media\":[{\"gif\":{\"dims\":[498,373],\"url\":\"https://media.tenor.com/images/2e284f337d67f844549974e7d6ffbfa9/tenor.gif\",\"preview\":\"https://media.tenor.com/images/4797252db01f9f65d9265f9e20e46276/tenor.png\",\"size\":7655676},\"mp4\":{\"url\":\"https://media.tenor.com/videos/8752fbfcd6a5eee7bd2147355d746305/mp4\",\"size\":1950173,\"dims\":[640,480],\"preview\":\"https://media.tenor.com/images/4797252db01f9f65d9265f9e20e46276/tenor.png\",\"duration\":4.2},\"tinygif\":{\"url\":\"https://media.tenor.com/images/c35d09bc87d877f0896297b4816d8932/tenor.gif\",\"size\":426836,\"preview\":\"https://media.tenor.com/images/97910207ccda8f75d6cb7f2a82359308/tenor.gif\",\"dims\":[220,165]}}],\"bg_color\":\"\",\"created\":1602517325.426405,\"itemurl\":\"https://tenor.com/view/jokowi-kaget-gif-18777951\",\"url\":\"https://tenor.com/bqXal.gif\",\"tags\":[],\"flags\":[],\"shares\":1,\"hasaudio\":false,\"hascaption\":false,\"source_id\":\"\",\"composite\":null},{\"id\":\"6186965\",\"title\":\"Jokowi - Bukan Urusan Saya\",\"h1_title\":\"\",\"media\":[{\"tinygif\":{\"preview\":\"https://media.tenor.com/images/f9c7c83bad2499192092492e83c9d21a/tenor.gif\",\"url\":\"https://media.tenor.com/images/62ff5a0eb420abcde947b6ddf9d1cf73/tenor.gif\",\"size\":25304,\"dims\":[220,202]},\"mp4\":{\"url\":\"https://media.tenor.com/videos/d73b5f6c9acb7ce2e4f1447b42809db3/mp4\",\"dims\":[250,230],\"duration\":0.4,\"size\":13371,\"preview\":\"https://media.tenor.com/images/da461d1d044dcef033f3c1b27107f026/tenor.png\"},\"gif\":{\"dims\":[250,230],\"preview\":\"https://media.tenor.com/images/da461d1d044dcef033f3c1b27107f026/tenor.png\",\"url\":\"https://media.tenor.com/images/3568ced8c78e34bd82cc4dc43da9a077/tenor.gif\",\"size\":93432}}],\"bg_color\":\"\",\"created\":1476776887.134904,\"itemurl\":\"https://tenor.com/view/bukan-urusan-saya-jokowi-bukan-urusan-gue-terserah-peduli-amat-gif-6186965\",\"url\":\"https://tenor.com/z7FV.gif\",\"tags\":[],\"flags\":[],\"shares\":1,\"hasaudio\":false,\"hascaption\":false,\"source_id\":\"\",\"composite\":null},{\"id\":\"7849350\",\"title\":\"Jokowi lawan begal\",\"h1_title\":\"\",\"media\":[{\"mp4\":{\"preview\":\"https://media.tenor.com/images/1631507a774b822c16c70563ff78032a/tenor.png\",\"dims\":[250,250],\"size\":14108,\"duration\":0.4,\"url\":\"https://media.tenor.com/videos/adb1a11a447455800e2d7d6258250893/mp4\"},\"gif\":{\"size\":119850,\"dims\":[250,250],\"preview\":\"https://media.tenor.com/images/1631507a774b822c16c70563ff78032a/tenor.png\",\"url\":\"https://media.tenor.com/images/3756b1d836e52e8237444cec4d0d3a03/tenor.gif\"},\"tinygif\":{\"preview\":\"https://media.tenor.com/images/9732364f5687191a5b1b0aca1f95a6d9/tenor.gif\",\"url\":\"https://media.tenor.com/images/0af2bf9728b31e39f2a4f063488fca7e/tenor.gif\",\"dims\":[220,220],\"size\":30596}}],\"bg_color\":\"\",\"created\":1487753031.209784,\"itemurl\":\"https://tenor.com/view/begal-jokowi-pakpres-indonesia-humor-gif-7849350\",\"url\":\"https://tenor.com/G58A.gif\",\"tags\":[],\"flags\":[],\"shares\":1,\"hasaudio\":false,\"hascaption\":false,\"source_id\":\"\",\"composite\":null},{\"id\":\"5963581\",\"title\":\"Ekspresi lucu Jokowi\",\"h1_title\":\"\",\"media\":[{\"tinygif\":{\"size\":148365,\"url\":\"https://media.tenor.com/images/de2551fc393f799c29b38b30ed5c54de/tenor.gif\",\"dims\":[220,142],\"preview\":\"https://media.tenor.com/images/a07fa6812a7443e4e6c8f2e66f858c9b/tenor.gif\"},\"gif\":{\"url\":\"https://media.tenor.com/images/9617de6e2da51bbad9bd09a117df03b4/tenor.gif\",\"preview\":\"https://media.tenor.com/images/15161833ad232d117402a30d79429b5d/tenor.png\",\"size\":242615,\"dims\":[216,140]},\"mp4\":{\"duration\":1.1,\"url\":\"https://media.tenor.com/videos/9e268d4540528b0042dbb888e1398324/mp4\",\"dims\":[216,140],\"preview\":\"https://media.tenor.com/images/15161833ad232d117402a30d79429b5d/tenor.png\",\"size\":25841}}],\"bg_color\":\"\",\"created\":1473311568.029695,\"itemurl\":\"https://tenor.com/view/jokowi-lucu-president-laugh-ketawa-gif-5963581\",\"url\":\"https://tenor.com/zbyX.gif\",\"tags\":[],\"flags\":[],\"shares\":1,\"hasaudio\":false,\"hascaption\":false,\"source_id\":\"\",\"composite\":null},{\"id\":\"11911066\",\"title\":\"JOKOWI DAN KAESANG\",\"h1_title\":\"\",\"media\":[{\"tinygif\":{\"dims\":[220,124],\"size\":137472,\"url\":\"https://media.tenor.com/images/a52ec76597b3daf20f891b62e7b237ac/tenor.gif\",\"preview\":\"https://media.tenor.com/images/c02b9d6118e3b0cec2aebef68291e6cf/tenor.gif\"},\"mp4\":{\"duration\":2.1,\"preview\":\"https://media.tenor.com/images/7f1a71676a16e835cb7a3cc6aa307bd3/tenor.png\",\"url\":\"https://media.tenor.com/videos/9c4394ed230c74788884f1d780b36c32/mp4\",\"size\":154605,\"dims\":[480,270]},\"gif\":{\"preview\":\"https://media.tenor.com/images/7f1a71676a16e835cb7a3cc6aa307bd3/tenor.png\",\"size\":1636353,\"url\":\"https://media.tenor.com/images/091421cf406c2bad820be936fb0b745b/tenor.gif\",\"dims\":[480,270]}}],\"bg_color\":\"\",\"created\":1527669516.019816,\"itemurl\":\"https://tenor.com/view/jokowi-kaesang-presiden-indonesia-presiden-president-gif-11911066\",\"url\":\"https://tenor.com/X8L8.gif\",\"tags\":[],\"flags\":[],\"shares\":1,\"hasaudio\":false,\"hascaption\":false,\"source_id\":\"\",\"composite\":null},{\"id\":\"17607762\",\"title\":\"\",\"h1_title\":\"\",\"media\":[{\"gif\":{\"dims\":[498,469],\"size\":2552443,\"url\":\"https://media.tenor.com/images/ccfba75f15aeba5ce645cef71e9ed98e/tenor.gif\",\"preview\":\"https://media.tenor.com/images/e03bccca4c769a97bba904e9e07f1730/tenor.png\"},\"tinygif\":{\"size\":155210,\"dims\":[220,208],\"preview\":\"https://media.tenor.com/images/d5385f066be432b81617116dcc9dae6d/tenor.gif\",\"url\":\"https://media.tenor.com/images/053af2b144939c9f6e72ec67d3f18a8d/tenor.gif\"},\"mp4\":{\"url\":\"https://media.tenor.com/videos/3a2fb06e54919387fd699b2fc444f532/mp4\",\"dims\":[640,604],\"size\":618308,\"preview\":\"https://media.tenor.com/images/e03bccca4c769a97bba904e9e07f1730/tenor.png\",\"duration\":2.7}}],\"bg_color\":\"\",\"created\":1593086427.114732,\"itemurl\":\"https://tenor.com/view/jokowi-ngutang-joko-widodo-i-owe-you-more-gif-17607762\",\"url\":\"https://tenor.com/bl2Kk.gif\",\"tags\":[],\"flags\":[],\"shares\":1,\"hasaudio\":false,\"hascaption\":false,\"source_id\":\"\",\"composite\":null},{\"id\":\"13472337\",\"title\":\"\",\"h1_title\":\"\",\"media\":[{\"mp4\":{\"url\":\"https://media.tenor.com/videos/ddab0bb0f92f054bf708248589acd080/mp4\",\"size\":471130,\"duration\":4,\"preview\":\"https://media.tenor.com/images/ba3911c03e6dff01ae87e1bfcbbbb998/tenor.png\",\"dims\":[640,360]},\"gif\":{\"url\":\"https://media.tenor.com/images/a1c344340e3b0878c684dfe4283d57c6/tenor.gif\",\"preview\":\"https://media.tenor.com/images/ba3911c03e6dff01ae87e1bfcbbbb998/tenor.png\",\"size\":2949509,\"dims\":[498,280]},\"tinygif\":{\"url\":\"https://media.tenor.com/images/f63f81563dc313ef50eb1c8525af6bbd/tenor.gif\",\"size\":221854,\"dims\":[220,124],\"preview\":\"https://media.tenor.com/images/64ef8751d6e34a1ed5e757b4c9392046/tenor.gif\"}}],\"bg_color\":\"\",\"created\":1549937448.423301,\"itemurl\":\"https://tenor.com/view/jokowi-goyang-dayung-prabowo-sweep-gif-13472337\",\"url\":\"https://tenor.com/4GVV.gif\",\"tags\":[],\"flags\":[],\"shares\":1,\"hasaudio\":true,\"hascaption\":false,\"source_id\":\"\",\"composite\":null},{\"id\":\"19275483\",\"title\":\"\",\"h1_title\":\"\",\"media\":[{\"gif\":{\"preview\":\"https://media.tenor.com/images/2c4a51f0f31751859ec277d09c13d2db/tenor.png\",\"url\":\"https://media.tenor.com/images/43452a7fbfecd396440d74cda6dd8462/tenor.gif\",\"dims\":[498,410],\"size\":4199733},\"tinygif\":{\"dims\":[220,182],\"url\":\"https://media.tenor.com/images/4c4dcf1161a5157a092c5dea436e6c1b/tenor.gif\",\"size\":92033,\"preview\":\"https://media.tenor.com/images/34156f22d0dc6ea13ddd606fc3b7b8a7/tenor.gif\"},\"mp4\":{\"dims\":[640,528],\"size\":459382,\"url\":\"https://media.tenor.com/videos/83c518aec20277764942b70aee2f7d56/mp4\",\"duration\":1.8,\"preview\":\"https://media.tenor.com/images/2c4a51f0f31751859ec277d09c13d2db/tenor.png\"}}],\"bg_color\":\"\",\"created\":1605921137.584358,\"itemurl\":\"https://tenor.com/view/kita-harus-selalu-optimis-jokowi-joko-widodo-boy-william-starhits-gif-19275483\",\"url\":\"https://tenor.com/bs2A3.gif\",\"tags\":[],\"flags\":[],\"shares\":1,\"hasaudio\":false,\"hascaption\":false,\"source_id\":\"\",\"composite\":null},{\"id\":\"11922911\",\"title\":\"Presiden ngakak\",\"h1_title\":\"\",\"media\":[{\"tinygif\":{\"dims\":[220,164],\"preview\":\"https://media.tenor.com/images/8724e1af6cc894573214cfb94f75e476/tenor.gif\",\"url\":\"https://media.tenor.com/images/034a9231edaeb4a6c5c8f45cbd95d1f7/tenor.gif\",\"size\":271647},\"mp4\":{\"duration\":2.6,\"preview\":\"https://media.tenor.com/images/12697a40c27ce2d147b9d32f766be1bd/tenor.png\",\"size\":218965,\"url\":\"https://media.tenor.com/videos/6ea7ecc900e898c922432366627a40ec/mp4\",\"dims\":[422,314]},\"gif\":{\"dims\":[422,314],\"size\":2052561,\"url\":\"https://media.tenor.com/images/89bd742cbfdf0742019da098872e1bf0/tenor.gif\",\"preview\":\"https://media.tenor.com/images/12697a40c27ce2d147b9d32f766be1bd/tenor.png\"}}],\"bg_color\":\"\",\"created\":1527825128.661565,\"itemurl\":\"https://tenor.com/view/jokowi-joko-widodo-president-ri1-indonesia-gif-11922911\",\"url\":\"https://tenor.com/YbRb.gif\",\"tags\":[],\"flags\":[],\"shares\":1,\"hasaudio\":false,\"hascaption\":false,\"source_id\":\"\",\"composite\":null},{\"id\":\"19275476\",\"title\":\"\",\"h1_title\":\"\",\"media\":[{\"gif\":{\"url\":\"https://media.tenor.com/images/90ad25e99f87eda2058bf7fc286287fc/tenor.gif\",\"size\":2282079,\"dims\":[498,298],\"preview\":\"https://media.tenor.com/images/8e1581082942ec6d18986c0f4db9586d/tenor.png\"},\"mp4\":{\"preview\":\"https://media.tenor.com/images/8e1581082942ec6d18986c0f4db9586d/tenor.png\",\"duration\":1.4,\"dims\":[640,384],\"size\":337203,\"url\":\"https://media.tenor.com/videos/d8cce9c4cb5ed47b01f8e7ccfc7fb79f/mp4\"},\"tinygif\":{\"preview\":\"https://media.tenor.com/images/a8313dafff4311d454416fabbbf7fb12/tenor.gif\",\"dims\":[220,132],\"url\":\"https://media.tenor.com/images/cfd4ac9344f4db45698005980cab57b4/tenor.gif\",\"size\":75213}}],\"bg_color\":\"\",\"created\":1605921123.543231,\"itemurl\":\"https://tenor.com/view/harus-semangat-jokowi-joko-widodo-boy-william-starhits-gif-19275476\",\"url\":\"https://tenor.com/bs2AW.gif\",\"tags\":[],\"flags\":[],\"shares\":1,\"hasaudio\":false,\"hascaption\":false,\"source_id\":\"\",\"composite\":null},{\"id\":\"8046529\",\"title\":\"Jokowi joged\",\"h1_title\":\"\",\"media\":[{\"gif\":{\"dims\":[400,266],\"size\":2835495,\"preview\":\"https://media.tenor.com/images/236578d4bfcfe1cf7f6153608a66998b/tenor.png\",\"url\":\"https://media.tenor.com/images/a1e0ff1739cd3fef5260bee74280ca9d/tenor.gif\"},\"mp4\":{\"preview\":\"https://media.tenor.com/images/236578d4bfcfe1cf7f6153608a66998b/tenor.png\",\"size\":325857,\"url\":\"https://media.tenor.com/videos/b426e0d1a2b074cbaaf3ca2961b9dcd4/mp4\",\"duration\":4.8,\"dims\":[400,266]},\"tinygif\":{\"size\":326326,\"url\":\"https://media.tenor.com/images/cb7dd44b67c90c66ee7efa1d5923eacf/tenor.gif\",\"dims\":[220,146],\"preview\":\"https://media.tenor.com/images/3fde1423cae7849e74ce9eba8dd18d51/tenor.gif\"}}],\"bg_color\":\"\",\"created\":1489988601.608724,\"itemurl\":\"https://tenor.com/view/jokowi-joged-joget-pakpres-indonesia-gif-8046529\",\"url\":\"https://tenor.com/HVqT.gif\",\"tags\":[],\"flags\":[],\"shares\":1,\"hasaudio\":false,\"hascaption\":false,\"source_id\":\"\",\"composite\":null},{\"id\":\"16991363\",\"title\":\"\",\"h1_title\":\"\",\"media\":[{\"gif\":{\"preview\":\"https://media.tenor.com/images/7ac6399ab1187b409706383485f9bdb5/tenor.png\",\"dims\":[498,498],\"url\":\"https://media.tenor.com/images/6cffa2ba9802a414399d8b4e7efa6bf3/tenor.gif\",\"size\":9060888},\"mp4\":{\"size\":1838463,\"preview\":\"https://media.tenor.com/images/7ac6399ab1187b409706383485f9bdb5/tenor.png\",\"dims\":[640,640],\"duration\":5.3,\"url\":\"https://media.tenor.com/videos/bee17426aed3b53c7908a8cd12bb1d48/mp4\"},\"tinygif\":{\"preview\":\"https://media.tenor.com/images/8a758249346334b3ae55f601b9dfe5a1/tenor.gif\",\"url\":\"https://media.tenor.com/images/c2e58425cf580ff22cbc0232aac3f3b4/tenor.gif\",\"size\":509429,\"dims\":[220,220]}}],\"bg_color\":\"\",\"created\":1587706253.027222,\"itemurl\":\"https://tenor.com/view/ruwet-ribet-jokowi-bingung-bodoh-gif-16991363\",\"url\":\"https://tenor.com/bjsop.gif\",\"tags\":[],\"flags\":[],\"shares\":1,\"hasaudio\":false,\"hascaption\":false,\"source_id\":\"\",\"composite\":null}],\"next\":\"20\"}",
				testAsync: async httpClient => {
					Mock<IOptions<TenorOptions>> optionsAccessorMock = new();

					optionsAccessorMock
						.SetupGet(accessor => accessor.Value)
						.Returns(new TenorOptions { ApiKey = "ASDFG" });

					TenorClient tenorClient = new(
						httpClient: httpClient,
						tenorOptionsAccessor: optionsAccessorMock.Object);

					ImmutableList<(string Id, string Url, string PreviewUrl)> gifs = await tenorClient.SearchGifsAsync("jokowi", CancellationToken.None);

					gifs.Count.Should().Be(20);
					foreach ((string id, string url, string previewUrl) in gifs) {
						id.Should().NotBeNullOrEmpty();
						url.Should().NotBeNullOrEmpty();
						previewUrl.Should().NotBeNullOrEmpty();
					}
				});
		}

		[Fact]
		public void SearchGifsAsync_WithoutConfiguringApiKey_ShouldThrowException() {
			Mock<IOptions<TenorOptions>> optionsAccessorMock = new();

			optionsAccessorMock
				.SetupGet(accessor => accessor.Value)
				.Returns(new TenorOptions());

			using HttpClient httpClient = new();

			new Action([ExcludeFromCodeCoverage]() => {
				_ = new TenorClient(
					httpClient: httpClient,
					tenorOptionsAccessor: optionsAccessorMock.Object);
			}).Should().Throw<InvalidOperationException>();
		}

		[Fact]
		public async Task SearchGifsAsync_ReceivesNull_ReturnsEmptyArrayAsync() {
			await TestHttpClientUsingDummyContentAsync(
				content: "null",
				testAsync: async httpClient => {
					Mock<IOptions<TenorOptions>> optionsAccessorMock = new();

					optionsAccessorMock
						.SetupGet(accessor => accessor.Value)
						.Returns(new TenorOptions { ApiKey = "ASDFG" });

					TenorClient tenorClient = new(
						httpClient: httpClient,
						tenorOptionsAccessor: optionsAccessorMock.Object);

					ImmutableList<(string Id, string Url, string PreviewUrl)> gifs = await tenorClient.SearchGifsAsync("jokowi", CancellationToken.None);

					gifs.Count.Should().Be(0);
				});
		}

		[Fact]
		public async Task SearchGifsAsync_EncountersUnexpectedResponse_ReturnsEmptyArrayAsync() {
			await TestHttpClientUsingDummyContentAsync(
				content: "<p>Anda dilarang menelusuri hasil pencarian dengan kata kunci ini.</p>",
				testAsync: async httpClient => {
					Mock<IOptions<TenorOptions>> optionsAccessorMock = new();

					optionsAccessorMock
						.SetupGet(accessor => accessor.Value)
						.Returns(new TenorOptions { ApiKey = "ASDFG" });

					TenorClient tenorClient = new(
						httpClient: httpClient,
						tenorOptionsAccessor: optionsAccessorMock.Object);

					ImmutableList<(string Id, string Url, string PreviewUrl)> gifs = await tenorClient.SearchGifsAsync("jokowi", CancellationToken.None);

					gifs.Count.Should().Be(0);
				});
		}

		private static async Task TestHttpClientUsingDummyContentAsync(string content, Func<HttpClient, Task> testAsync) {
			Mock<HttpMessageHandler> handlerMock = new();

			using HttpResponseMessage responseMessage = new() {
				StatusCode = HttpStatusCode.OK,
				Content = new StringContent(content)
			};

			handlerMock
				.Protected()
				.Setup<Task<HttpResponseMessage>>(
					"SendAsync",
					ItExpr.IsAny<HttpRequestMessage>(),
					ItExpr.IsAny<CancellationToken>()
				)
				.ReturnsAsync(responseMessage);

			using HttpClient httpClient = new(handlerMock.Object);

			await testAsync(httpClient);
		}
	}
}

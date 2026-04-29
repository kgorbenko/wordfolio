namespace Wordfolio.Api.Tests.Infrastructure

open Microsoft.Extensions.Options

open Sqids

open Xunit

open Wordfolio.Api.Configuration.SqidsEncoder
open Wordfolio.Api.Infrastructure.ResourceIdEncoder

module ResourceIdEncoderTests =

    let private testAlphabet =
        "abcdefghijklmnopqrstuvwxyz"

    let private createEncoder() : IResourceIdEncoder =
        let configuration =
            { Alphabet = testAlphabet }

        let options = Options.Create(configuration)
        ResourceIdEncoder(options) :> IResourceIdEncoder

    [<Fact>]
    let ``valid encoded id returns Some id``() =
        let encoder = createEncoder()
        let encoded = encoder.Encode(42)
        let result = encoder.Decode(encoded)
        Assert.Equal(Some 42, result)

    [<Fact>]
    let ``zero is encoded and decoded correctly``() =
        let encoder = createEncoder()
        let encoded = encoder.Encode(0)
        let result = encoder.Decode(encoded)
        Assert.Equal(Some 0, result)

    [<Fact>]
    let ``multi-id encoding returns None``() =
        let encoder = createEncoder()

        let sqids =
            SqidsEncoder(SqidsOptions(Alphabet = testAlphabet))

        let multiIdEncoded =
            sqids.Encode([| 1; 2 |])

        let result = encoder.Decode(multiIdEncoded)
        Assert.Equal(None, result)

    [<Fact>]
    let ``non-canonical encoding returns None``() =
        let encoder = createEncoder()
        let result = encoder.Decode("aa")
        Assert.Equal(None, result)

    [<Fact>]
    let ``overflow to negative returns None without throwing``() =
        let encoder = createEncoder()

        let mutable decodeResult: int option =
            Some 0

        let ex =
            Record.Exception(fun () -> decodeResult <- encoder.Decode(String.replicate 32 "z"))

        Assert.Null(ex)
        Assert.Equal(None, decodeResult)

    [<Fact>]
    let ``empty string returns None``() =
        let encoder = createEncoder()
        let result = encoder.Decode("")
        Assert.Equal(None, result)

    [<Fact>]
    let ``out-of-alphabet characters return None``() =
        let encoder = createEncoder()
        let result = encoder.Decode("!!!")
        Assert.Equal(None, result)

using DeckBuilder.Api.Enums;
using MongoDB.Bson;

namespace DeckBuilder.Api.Services;

public static class CardServiceMongoPipelines
{
    public static BsonDocument MatchById(string cardId)
    {
        return new BsonDocument
        {
            {
                "$match", new BsonDocument
                {
                    { "_id", new ObjectId(cardId) }
                }
            }
        };
    }

    public static BsonDocument MatchBySetCode(string setCode)
    {
        return new BsonDocument("$match", new BsonDocument
        {
            { "attributes.key", "setCode" },
            { "attributes.value", setCode }
        });
    }

    public static BsonDocument MatchByCode(string code)
    {
        return new BsonDocument("$match", new BsonDocument
        {
            {
                "attributes", new BsonDocument("$elemMatch", new BsonDocument
                {
                    { "key", "code" },
                    { "value", code }
                })
            }
        });
    }
    
    public static BsonDocument MatchBySet(string setCode)
    {
        return new BsonDocument
        {
            {
                "$match", new BsonDocument
                {
                    {
                        "attributes", new BsonDocument(
                            "$elemMatch", new BsonDocument
                            {
                                { "key", "setCode" },
                                { "value", setCode }
                            }
                        )
                    }
                }
            }
        };
    }
    
    public static BsonDocument MatchIdsInArray(IEnumerable<string> cardIds)
    {
        return new BsonDocument
        {
            {
                "$match", new BsonDocument
                {
                    {
                        "_id", new BsonDocument
                        {
                            { "$in", new BsonArray(cardIds.Select(id => new ObjectId(id))) }
                        }
                    }
                }
            }
        };
    }
    
    public static BsonDocument ProjectCardWithDetails()
    {
        return new BsonDocument
        {
            {
                "$project", new BsonDocument
                {
                    { "_id", 1 },
                    { "language", 1 },
                    {
                        "attributes", new BsonDocument("$arrayToObject", new BsonDocument("$map", new BsonDocument
                        {
                            { "input", "$attributes" },
                            { "as", "attr" },
                            {
                                "in", new BsonArray
                                {
                                    new BsonDocument("$toString", "$$attr.key"),
                                    new BsonDocument("$cond", new BsonDocument
                                    {
                                        {
                                            "if", new BsonDocument("$eq", new BsonArray
                                            {
                                                new BsonDocument("$size", "$$attr.values"),
                                                0
                                            })
                                        },
                                        { "then", "$$attr.value" },
                                        { "else", "$$attr.values" }
                                    })
                                }
                            }
                        }))
                    }
                }
            }
        };
    }

    public static BsonDocument MatchByRaritiesWithSampleSize(string[] rarities)
    {
        return new BsonDocument("$match", new BsonDocument
        {
            {
                "attributes", new BsonDocument("$elemMatch", new BsonDocument
                {
                    { "key", "rarityCode" },
                    { "value", new BsonDocument("$in", new BsonArray(rarities)) }
                })
            }
        });
    }

    public static BsonDocument SampleWithSize(int sampleSize)
    {
        return new BsonDocument("$sample", new BsonDocument { { "size", sampleSize } });
    }


    public static BsonDocument FacetCardsByRarities()
    {
        return new BsonDocument("$facet", new BsonDocument
        {
            {
                "rareOrPremium", new BsonArray
                {
                    MatchByRaritiesWithSampleSize([Rarities.Rare, Rarities.FoilPremium, Rarities.HoloPortraitPremium]),
                    SampleWithSize(1)
                }
            },
            {
                "uncommon", new BsonArray
                {
                    MatchByRaritiesWithSampleSize([Rarities.Uncommon]),
                    SampleWithSize(2)
                }
            },
            {
                "common", new BsonArray
                {

                    MatchByRaritiesWithSampleSize([Rarities.Common]),
                    SampleWithSize(12)
                }
            }
        });
    }

    public static BsonDocument[] ProjectFacetToRoot()
    {
        return
        [
            new BsonDocument("$project", new BsonDocument
            {
                { "packCards", new BsonDocument("$concatArrays", new BsonArray { "$rareOrPremium", "$uncommon", "$common" }) }
            }),
            new BsonDocument("$unwind", "$packCards"),
            new BsonDocument("$replaceRoot", new BsonDocument
            {
                { "newRoot", "$packCards" }
            }),
            new BsonDocument("$group", new BsonDocument
            {
                { "_id", "$_id" },
                { "card", new BsonDocument("$first", "$$ROOT") }
            }),
            new BsonDocument("$replaceRoot", new BsonDocument
            {
                { "newRoot", "$card" }
            })
        ];
    }
}
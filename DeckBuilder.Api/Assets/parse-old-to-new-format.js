const fs = require('fs');
const {ObjectId} = require('mongodb');

const oldPath = './old';
const newPath = './new';
const oldDirectories = fs.readdirSync(oldPath);

for (const oldDirectory of oldDirectories) {
    const newDirectory = `${newPath}/${oldDirectory}`;
    
    if (fs.existsSync(newDirectory)) {
        fs.rmSync(newDirectory, { recursive: true });    
    }
    
    fs.mkdirSync(newDirectory);
    
    const oldFiles = fs.readdirSync(`${oldPath}/${oldDirectory}`);
    const cardFiles = oldFiles.filter(f => !f.includes('game.json') && !f.includes('images'));
    const gameFilePath = `${newPath}/${oldDirectory}/game.json`;
    
    const gameContent = fs.readFileSync(`${oldPath}/${oldDirectory}/game.json`, 'utf-8');

    if (fs.existsSync(gameFilePath)) {
        fs.rmSync(gameFilePath);
    }

    fs.writeFileSync(gameFilePath, gameContent);
    
    for (const oldFileName of cardFiles) {
        const fileContent = fs.readFileSync(`${oldPath}/${oldDirectory}/${oldFileName}`, 'utf-8');
        const newFilePath = `${newPath}/${oldDirectory}/${oldFileName}`;

        if (fs.existsSync(newFilePath)) {
            fs.rmSync(newFilePath);
        }
        
        const oldCards = JSON.parse(fileContent);
        const newCards = oldCards.map((card) => convertOldToNewFormat(card, JSON.parse(gameContent).id));
        
        fs.writeFileSync(newFilePath, JSON.stringify(newCards, null, 2));
    }
}

function convertOldToNewFormat(oldObject, gameId) {
    const attributes = [{
        key: 'name',
        value: oldObject.detail?.name,
        searchable: "true",
        visible: "true",
    }, {
        key: 'code',
        value: oldObject.detail?.name
            .toLowerCase()
            .replace(/[^a-z0-9]+/g, '-')
            .replace(/^-+|-+$/g, ''),
        searchable: "true",
        visible: "true",
    },{
        key: 'text',
        value: oldObject.detail?.text,
        searchable: "true",
        visible: "true",
    }, {
        key: 'effect',
        value: oldObject.detail?.effect,
        searchable: "true",
        visible: "true",
    }, {
        key: 'toSolve',
        value: oldObject.detail?.toSolve,
        searchable: "true",
        visible: "true",
    }, {
        key: 'reward',
        value: oldObject.detail?.reward,
        searchable: "true",
        visible: "true",
    }, {
        key: 'flavorText',
        value: oldObject.detail?.flavorText,
        searchable: "true",
        visible: "true",
    }, {
        key: 'illustrator',
        value: oldObject.detail?.illustrator,
        searchable: "true",
        visible: "true",
    }, {
        key: 'copyright',
        value: oldObject.detail?.copyright,
        searchable: "true",
    }, {
        key: 'note',
        value: oldObject.detail?.note,
        searchable: "true",
        visible: "true",
    }, {
        key: 'orientation',
        value: oldObject.detail?.orientation,
        searchable: "true",
        visible: "true",
    }, {
        key: 'set',
        value: oldObject.cardSet?.name,
        searchable: "true",
        visible: "true",
    }, {
        key: 'setCode',
        value: oldObject.cardSet?.shortName?.toLowerCase(),
        searchable: "true",
        visible: "false",
    }, {
        key: 'type',
        value: oldObject.cardType?.name,
        searchable: "true",
        visible: "true",
    },  {
        key: 'typeCode',
        value: oldObject.cardType?.name.replace(/\s+/g, '-').toLowerCase(),
        searchable: "true",
        visible: "true",
    }, {
        key: 'rarity',
        value: oldObject.rarity?.name,
        searchable: "true",
        visible: "true",
    }, {
        key: 'rarityCode',
        value: oldObject.rarity?.name.replace(/\s+/g, '-').toLowerCase(),
        searchable: "true",
        visible: "true",
    }, {
        key: 'lessonType',
        value: oldObject.lessonType?.name,
        searchable: "true",
        visible: "true",
    }, {
        key: 'lessonTypeCode',
        value: oldObject.lessonType?.name.replace(/\s+/g, '-').toLowerCase(),
        searchable: "true",
        visible: "true",
    }, {
        key: 'lessonCost',
        value: oldObject.lessonCost?.toString(),
        searchable: "true",
        visible: "true",
    }, {
        key: 'actionCost',
        value: oldObject.actionCost?.toString(),
        searchable: "true",
        visible: "true",
    }, {
        key: 'cardNumber',
        value: oldObject.cardNumber?.toString(),
        searchable: "true",
        visible: "true",
    }, {
        key: 'health',
        value: oldObject.health?.toString(),
        searchable: "true",
        visible: "true",
    }, {
        key: 'damage',
        value: oldObject.damage?.toString(),
        searchable: "true",
        visible: "true",
    }, {
        key: 'providesLesson',
        value: oldObject.providesLesson?.lesson?.name,
        searchable: "true",
        visible: "true",
    }, {
        key: 'subType',
        values: oldObject.subTypes?.map(({ subType }) => subType.name),
        searchable: "true",
        visible: "true",
    }];

    return {
        id: new ObjectId(),
        gameId,
        language: "en",
        attributes: attributes.filter((attribute) => {
            return !!attribute.value || !!attribute.values;
        })
    };
}

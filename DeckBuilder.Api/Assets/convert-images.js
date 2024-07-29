const fs = require('fs');

const oldPath = './old';
const newPath = './new';

const oldDirectories = fs.readdirSync(oldPath);
for (const oldDirectory of oldDirectories) {
    const path = `${oldPath}/${oldDirectory}`;
    if (!fs.lstatSync(path).isDirectory()) {
        continue;
    }
    
    const newImagesPath = `${newPath}/${oldDirectory}`;
    
    if (!fs.existsSync(newPath)) {
        fs.mkdirSync(newPath);
    }

    const oldFilePath = `${path}/images`;
    const newFilePath = `${newImagesPath}/images`;
    const oldFiles = fs.readdirSync(oldFilePath);
    
    if (!fs.existsSync(newFilePath)) {
        fs.mkdirSync(newFilePath);
    }

    for (const oldFile of oldFiles) {
        const newFileName = oldFile
            .replace(/([A-Z])/g, '-$1')
            .toLowerCase()
            .replace(/^-/, '');
        
        fs.copyFileSync(`${oldFilePath}/${oldFile}`, `${newFilePath}/${newFileName}`);
    }
}
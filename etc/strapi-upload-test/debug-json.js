const fs = require('fs');
const path = require('path');

// 模擬我們 C# 的 JSON 序列化
const fileInfo = {
  name: "test-upload.png",
  alternativeText: "測試文件",
  caption: "上傳測試用的文件"
};

console.log('我們的 JSON 輸出:');
console.log(JSON.stringify(fileInfo));

console.log('\nNode.js 成功的 JSON 輸出:');
console.log(JSON.stringify({
  name: 'test-upload-with-info.png',
  alternativeText: '這是測試圖片',
  caption: '測試標題'
}));
const fs = require('fs');
const path = require('path');
const FormData = require('form-data');
const fetch = require('node-fetch');

async function inspectFormData() {
  console.log('=== 檢查 Node.js FormData 構建 ===\n');
  
  // 創建測試圖片
  const testImagePath = path.join(__dirname, 'test-image.png');
  const pngBuffer = Buffer.from([
    0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A,
    0x00, 0x00, 0x00, 0x0D, 0x49, 0x48, 0x44, 0x52,
    0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x01,
    0x08, 0x06, 0x00, 0x00, 0x00, 0x1F, 0x15, 0xC4, 0x89,
    0x00, 0x00, 0x00, 0x0A, 0x49, 0x44, 0x41, 0x54,
    0x78, 0x9C, 0x63, 0x00, 0x01, 0x00, 0x00, 0x05, 0x00, 0x01,
    0x0D, 0x0A, 0x2D, 0xB4, 0x00, 0x00, 0x00, 0x00,
    0x49, 0x45, 0x4E, 0x44, 0xAE, 0x42, 0x60, 0x82
  ]);
  fs.writeFileSync(testImagePath, pngBuffer);
  
  // 建立表單（與成功的測試相同）
  const form = new FormData();
  form.append('files', fs.createReadStream(testImagePath), 'test-upload.png');
  form.append('fileInfo', JSON.stringify({
    name: 'test-upload.png',
    alternativeText: 'Test Image',
    caption: 'Test Caption'
  }));
  
  // 檢查 headers
  console.log('📋 Node.js Headers:');
  const headers = form.getHeaders();
  for (const [key, value] of Object.entries(headers)) {
    console.log(`   ${key}: ${value}`);
  }
  
  // 檢查表單字段
  console.log('\n📦 Node.js FormData 字段:');
  console.log('   - files: 檔案流');
  console.log('   - fileInfo: JSON 字串');
  
  // 檢查 boundary
  const boundary = headers['content-type'].match(/boundary=(.+)/)[1];
  console.log('\n🔍 Boundary:', boundary);
  
  // 模擬表單結構
  const fileInfoJson = JSON.stringify({
    name: 'test-upload.png',
    alternativeText: 'Test Image',
    caption: 'Test Caption'
  });
  
  console.log('📝 fileInfo JSON 內容:');
  console.log('   原始:', fileInfoJson);
  console.log('   長度:', fileInfoJson.length);
  console.log('   UTF-8 位元組:', Buffer.from(fileInfoJson, 'utf8').length);
  
  // 清理
  fs.unlinkSync(testImagePath);
  console.log('\n✅ Node.js 檢查完成');
}

// 創建一個 C# 風格的測試來比較
async function createCSharpStyleForm() {
  console.log('\n=== 模擬 C# MultipartFormDataContent ===\n');
  
  const testImagePath = path.join(__dirname, 'test-image-cs.png');
  const pngBuffer = Buffer.from([
    0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A,
    0x00, 0x00, 0x00, 0x0D, 0x49, 0x48, 0x44, 0x52,
    0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x01,
    0x08, 0x06, 0x00, 0x00, 0x00, 0x1F, 0x15, 0xC4, 0x89,
    0x00, 0x00, 0x00, 0x0A, 0x49, 0x44, 0x41, 0x54,
    0x78, 0x9C, 0x63, 0x00, 0x01, 0x00, 0x00, 0x05, 0x00, 0x01,
    0x0D, 0x0A, 0x2D, 0xB4, 0x00, 0x00, 0x00, 0x00,
    0x49, 0x45, 0x4E, 0x44, 0xAE, 0x42, 0x60, 0x82
  ]);
  fs.writeFileSync(testImagePath, pngBuffer);
  
  // 嘗試不同的 Content-Type 設定
  const form = new FormData();
  form.append('files', fs.createReadStream(testImagePath), {
    filename: 'test-upload.png',
    contentType: 'image/png'
  });
  
  // 模擬 C# 中可能的字串內容設定方式
  const jsonString = JSON.stringify({
    name: 'test-upload.png',
    alternativeText: 'Test Image',
    caption: 'Test Caption'
  });
  
  console.log('JSON 字串:', jsonString);
  console.log('JSON 字串長度:', jsonString.length);
  console.log('JSON 字串 UTF-8 位元組:', Buffer.from(jsonString, 'utf8').length);
  
  // 嘗試加入不同的 Content-Type
  form.append('fileInfo', jsonString, {
    contentType: 'application/json'
  });
  
  console.log('\n📋 C# 風格 Headers:');
  const headers = form.getHeaders();
  for (const [key, value] of Object.entries(headers)) {
    console.log(`   ${key}: ${value}`);
  }
  
  // 測試上傳
  const API_TOKEN = 'bd8cdd66daecf5db8dbdfbeccbbf4e4adba0a38834f72a66700e31b1ad5864051a3ede3c0f028aae7621ef3077cc2226ed6e61e8c68c80f58d53d70d2ac64f8c401ca0c004378a8d62480ea78190eb9c505571c1b538f659a4a06e8c39e5a1c39ede18dfb600b11511b4f61c84b9fbe92e77a90340122a79e98d8ef9915fb5f1';
  
  console.log('\n🧪 測試 C# 風格表單上傳:');
  const response = await fetch('http://localhost:1337/api/upload', {
    method: 'POST',
    body: form,
    headers: {
      ...form.getHeaders(),
      'Authorization': `Bearer ${API_TOKEN}`
    }
  });
  
  if (response.ok) {
    const result = await response.json();
    console.log('✅ C# 風格上傳成功:');
    console.log('   - alternativeText:', result[0].alternativeText);
    console.log('   - caption:', result[0].caption);
    console.log('   - name:', result[0].name);
  } else {
    console.log('❌ C# 風格上傳失敗:', response.status, await response.text());
  }
  
  fs.unlinkSync(testImagePath);
}

// 執行測試
async function runTests() {
  await inspectFormData();
  await createCSharpStyleForm();
}

runTests().catch(console.error);
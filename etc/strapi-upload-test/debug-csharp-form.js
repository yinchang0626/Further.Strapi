const fs = require('fs');
const path = require('path');
const FormData = require('form-data');
const fetch = require('node-fetch');

async function debugCSharpForm() {
  console.log('=== 調試 C# 表單問題 ===\n');
  
  // 創建測試圖片
  const testImagePath = path.join(__dirname, 'debug-test.png');
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
  
  const API_TOKEN = 'bd8cdd66daecf5db8dbdfbeccbbf4e4adba0a38834f72a66700e31b1ad5864051a3ede3c0f028aae7621ef3077cc2226ed6e61e8c68c80f58d53d70d2ac64f8c401ca0c004378a8d62480ea78190eb9c505571c1b538f659a4a06e8c39e5a1c39ede18dfb600b11511b4f61c84b9fbe92e77a90340122a79e98d8ef9915fb5f1';
  
  // 測試 1: 原來成功的方式
  console.log('🧪 測試 1: 原來成功的方式 (直接添加 JSON 字串)');
  const form1 = new FormData();
  form1.append('files', fs.createReadStream(testImagePath), 'test-debug.png');
  form1.append('fileInfo', JSON.stringify({
    name: 'test-debug.png',
    alternativeText: '成功測試',
    caption: '成功標題'
  }));
  
  const response1 = await fetch('http://localhost:1337/api/upload', {
    method: 'POST',
    body: form1,
    headers: {
      ...form1.getHeaders(),
      'Authorization': `Bearer ${API_TOKEN}`
    }
  });
  
  if (response1.ok) {
    const result1 = await response1.json();
    console.log('✅ 測試 1 成功:');
    console.log('   - alternativeText:', result1[0].alternativeText);
    console.log('   - caption:', result1[0].caption);
  } else {
    console.log('❌ 測試 1 失敗:', response1.status);
  }
  
  // 測試 2: 模擬 C# StringContent 的行為
  console.log('\n🧪 測試 2: 模擬 C# StringContent');
  const form2 = new FormData();
  form2.append('files', fs.createReadStream(testImagePath), 'test-debug2.png');
  
  // 模擬 C# 中的 StringContent 行為
  const jsonString = JSON.stringify({
    name: 'test-debug2.png',
    alternativeText: '測試文件',
    caption: '上傳測試用的文件'
  });
  
  // 直接以字串形式添加，不指定 content-type
  form2.append('fileInfo', jsonString);
  
  const response2 = await fetch('http://localhost:1337/api/upload', {
    method: 'POST',
    body: form2,
    headers: {
      ...form2.getHeaders(),
      'Authorization': `Bearer ${API_TOKEN}`
    }
  });
  
  if (response2.ok) {
    const result2 = await response2.json();
    console.log('✅ 測試 2 成功:');
    console.log('   - alternativeText:', result2[0].alternativeText);
    console.log('   - caption:', result2[0].caption);
  } else {
    console.log('❌ 測試 2 失敗:', response2.status);
  }
  
  // 測試 3: 嘗試不同的編碼
  console.log('\n🧪 測試 3: UTF-8 Buffer 方式');
  const form3 = new FormData();
  form3.append('files', fs.createReadStream(testImagePath), 'test-debug3.png');
  
  const jsonBuffer = Buffer.from(JSON.stringify({
    name: 'test-debug3.png',
    alternativeText: '測試文件',
    caption: '上傳測試用的文件'
  }), 'utf8');
  
  form3.append('fileInfo', jsonBuffer, {
    contentType: 'text/plain; charset=utf-8'
  });
  
  const response3 = await fetch('http://localhost:1337/api/upload', {
    method: 'POST',
    body: form3,
    headers: {
      ...form3.getHeaders(),
      'Authorization': `Bearer ${API_TOKEN}`
    }
  });
  
  if (response3.ok) {
    const result3 = await response3.json();
    console.log('✅ 測試 3 成功:');
    console.log('   - alternativeText:', result3[0].alternativeText);
    console.log('   - caption:', result3[0].caption);
  } else {
    console.log('❌ 測試 3 失敗:', response3.status);
  }
  
  // 測試 4: 嘗試 camelCase 轉換是否正確
  console.log('\n🧪 測試 4: 確保 camelCase 格式');
  const form4 = new FormData();
  form4.append('files', fs.createReadStream(testImagePath), 'test-debug4.png');
  
  // 確保使用正確的 camelCase 格式
  const camelCaseJson = JSON.stringify({
    name: 'test-debug4.png',
    alternativeText: '測試文件',  // camelCase
    caption: '上傳測試用的文件'
  });
  
  console.log('   JSON 內容:', camelCaseJson);
  form4.append('fileInfo', camelCaseJson);
  
  const response4 = await fetch('http://localhost:1337/api/upload', {
    method: 'POST',
    body: form4,
    headers: {
      ...form4.getHeaders(),
      'Authorization': `Bearer ${API_TOKEN}`
    }
  });
  
  if (response4.ok) {
    const result4 = await response4.json();
    console.log('✅ 測試 4 成功:');
    console.log('   - alternativeText:', result4[0].alternativeText);
    console.log('   - caption:', result4[0].caption);
  } else {
    console.log('❌ 測試 4 失敗:', response4.status);
  }
  
  // 清理
  fs.unlinkSync(testImagePath);
  console.log('\n✅ 調試完成，檔案已清理');
}

debugCSharpForm().catch(console.error);
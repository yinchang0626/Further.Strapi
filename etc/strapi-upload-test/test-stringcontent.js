const fs = require('fs');
const path = require('path');
const FormData = require('form-data');
const fetch = require('node-fetch');

async function testStringContentBehavior() {
  console.log('=== 測試 StringContent 行為差異 ===\n');
  
  // 創建測試圖片
  const testImagePath = path.join(__dirname, 'stringcontent-test.png');
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
  
  // 測試 1: C# StringContent() 默認行為 (text/plain; charset=utf-8)
  console.log('🧪 測試 1: 模擬 C# StringContent() 默認 Content-Type');
  const form1 = new FormData();
  form1.append('files', fs.createReadStream(testImagePath), 'test1.png');
  
  const jsonString1 = JSON.stringify({
    name: 'test1.png',
    alternativeText: '測試1',
    caption: '標題1'
  });
  
  // 模擬 C# StringContent() 的默認 Content-Type
  form1.append('fileInfo', jsonString1, {
    contentType: 'text/plain; charset=utf-8'
  });
  
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
    console.log('   結果:');
    console.log('   - alternativeText:', result1[0].alternativeText);
    console.log('   - caption:', result1[0].caption);
  } else {
    console.log('   失敗:', response1.status);
  }
  
  // 測試 2: 完全不指定任何 Content-Type
  console.log('\n🧪 測試 2: 完全不指定 Content-Type');
  const form2 = new FormData();
  form2.append('files', fs.createReadStream(testImagePath), 'test2.png');
  
  const jsonString2 = JSON.stringify({
    name: 'test2.png',
    alternativeText: '測試2',
    caption: '標題2'
  });
  
  // 完全不指定 Content-Type
  form2.append('fileInfo', jsonString2);
  
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
    console.log('   結果:');
    console.log('   - alternativeText:', result2[0].alternativeText);
    console.log('   - caption:', result2[0].caption);
  } else {
    console.log('   失敗:', response2.status);
  }
  
  // 測試 3: 模擬 C# StringContent(string, Encoding.UTF8) 行為
  console.log('\n🧪 測試 3: 模擬 C# StringContent(string, Encoding.UTF8)');
  const form3 = new FormData();
  form3.append('files', fs.createReadStream(testImagePath), 'test3.png');
  
  const jsonString3 = JSON.stringify({
    name: 'test3.png',
    alternativeText: '測試3',
    caption: '標題3'
  });
  
  // 這可能更接近 C# StringContent(string, Encoding.UTF8) 的行為
  const utf8Buffer = Buffer.from(jsonString3, 'utf8');
  form3.append('fileInfo', utf8Buffer, {
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
    console.log('   結果:');
    console.log('   - alternativeText:', result3[0].alternativeText);
    console.log('   - caption:', result3[0].caption);
  } else {
    console.log('   失敗:', response3.status);
  }
  
  // 測試 4: 嘗試 application/x-www-form-urlencoded (某些情況下的默認值)
  console.log('\n🧪 測試 4: application/x-www-form-urlencoded');
  const form4 = new FormData();
  form4.append('files', fs.createReadStream(testImagePath), 'test4.png');
  
  const jsonString4 = JSON.stringify({
    name: 'test4.png',
    alternativeText: '測試4',
    caption: '標題4'
  });
  
  form4.append('fileInfo', jsonString4, {
    contentType: 'application/x-www-form-urlencoded'
  });
  
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
    console.log('   結果:');
    console.log('   - alternativeText:', result4[0].alternativeText);
    console.log('   - caption:', result4[0].caption);
  } else {
    console.log('   失敗:', response4.status);
  }
  
  // 清理
  fs.unlinkSync(testImagePath);
  console.log('\n✅ 測試完成，檔案已清理');
}

testStringContentBehavior().catch(console.error);
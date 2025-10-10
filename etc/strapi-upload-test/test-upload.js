const fs = require('fs');
const path = require('path');
const FormData = require('form-data');
const fetch = require('node-fetch');

async function testUpload() {
  try {
    console.log('開始測試 Strapi 上傳 API...');
    
    // API Token (從測試配置)
    const API_TOKEN = 'bd8cdd66daecf5db8dbdfbeccbbf4e4adba0a38834f72a66700e31b1ad5864051a3ede3c0f028aae7621ef3077cc2226ed6e61e8c68c80f58d53d70d2ac64f8c401ca0c004378a8d62480ea78190eb9c505571c1b538f659a4a06e8c39e5a1c39ede18dfb600b11511b4f61c84b9fbe92e77a90340122a79e98d8ef9915fb5f1';
    
    // 創建一個小測試檔案
    const testImagePath = path.join(__dirname, 'test-image.png');
    
    // 建立一個最小的 PNG 檔案（1x1 像素）
    const pngBuffer = Buffer.from([
      0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, // PNG 簽名
      0x00, 0x00, 0x00, 0x0D, // IHDR 長度
      0x49, 0x48, 0x44, 0x52, // IHDR
      0x00, 0x00, 0x00, 0x01, // 寬度 1
      0x00, 0x00, 0x00, 0x01, // 高度 1
      0x08, 0x06, 0x00, 0x00, 0x00, // 位深度 8, 色彩類型 6 (RGBA), 壓縮方法 0, 濾波方法 0, 交錯方法 0
      0x1F, 0x15, 0xC4, 0x89, // CRC
      0x00, 0x00, 0x00, 0x0A, // IDAT 長度
      0x49, 0x44, 0x41, 0x54, // IDAT
      0x78, 0x9C, 0x63, 0x00, 0x01, 0x00, 0x00, 0x05, 0x00, 0x01, // 圖像數據
      0x0D, 0x0A, 0x2D, 0xB4, // CRC
      0x00, 0x00, 0x00, 0x00, // IEND 長度
      0x49, 0x45, 0x4E, 0x44, // IEND
      0xAE, 0x42, 0x60, 0x82  // CRC
    ]);
    
    fs.writeFileSync(testImagePath, pngBuffer);
    console.log('✅ 測試圖片檔案已建立');
    
    // 測試 1: 不帶 fileInfo
    console.log('\n🧪 測試 1: 上傳檔案（不帶 metadata）');
    const form1 = new FormData();
    form1.append('files', fs.createReadStream(testImagePath), 'test-upload.png');
    
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
      console.log('✅ 上傳成功 (無 metadata):');
      console.log('   - alternativeText:', result1[0].alternativeText);
      console.log('   - caption:', result1[0].caption);
      console.log('   - name:', result1[0].name);
      console.log('   - id:', result1[0].id);
    } else {
      console.log('❌ 上傳失敗:', response1.status, await response1.text());
      return;
    }
    
    // 測試 2: 帶 fileInfo JSON
    console.log('\n🧪 測試 2: 上傳檔案（帶 fileInfo JSON）');
    const form2 = new FormData();
    form2.append('files', fs.createReadStream(testImagePath), 'test-upload-with-info.png');
    form2.append('fileInfo', JSON.stringify({
      name: 'test-upload-with-info.png',
      alternativeText: '這是測試圖片',
      caption: '測試標題'
    }));
    
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
      console.log('✅ 上傳成功 (帶 metadata):');
      console.log('   - alternativeText:', result2[0].alternativeText);
      console.log('   - caption:', result2[0].caption);
      console.log('   - name:', result2[0].name);
      console.log('   - id:', result2[0].id);
    } else {
      console.log('❌ 上傳失敗:', response2.status, await response2.text());
    }
    
    // 測試 3: 帶個別字段格式
    console.log('\n🧪 測試 3: 上傳檔案（帶個別 metadata 字段）');
    const form3 = new FormData();
    form3.append('files', fs.createReadStream(testImagePath), 'test-upload-separate.png');
    form3.append('fileInfo.name', 'test-upload-separate.png');
    form3.append('fileInfo.alternativeText', '個別字段測試');
    form3.append('fileInfo.caption', '個別字段標題');
    
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
      console.log('✅ 上傳成功 (個別字段):');
      console.log('   - alternativeText:', result3[0].alternativeText);
      console.log('   - caption:', result3[0].caption);
      console.log('   - name:', result3[0].name);
      console.log('   - id:', result3[0].id);
    } else {
      console.log('❌ 上傳失敗:', response3.status, await response3.text());
    }
    
    // 清理測試檔案
    fs.unlinkSync(testImagePath);
    console.log('\n🗑️ 測試檔案已清理');
    
  } catch (error) {
    console.error('❌ 測試出錯:', error.message);
    console.error(error.stack);
  }
}

testUpload();
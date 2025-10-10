/**
 * Strapi Admin and API Token Setup Script for CI/CD
 * 自動建立管理員帳號和 API Token
 */

const axios = require('axios');
const fs = require('fs').promises;
const path = require('path');

const STRAPI_URL = 'http://localhost:1337';
const ADMIN_USER = {
  firstname: 'CI',
  lastname: 'Admin',
  email: 'ci-admin@test.com',
  password: 'CIAdmin123!',
  confirmPassword: 'CIAdmin123!'
};

const API_TOKEN_CONFIG = {
  name: 'ci-integration-test-token',
  description: 'API token for CI integration tests',
  type: 'full-access',
  permissions: null,
  lifespan: null
};

async function waitForStrapi(maxRetries = 30, interval = 2000) {
  console.log('🔍 Waiting for Strapi to be ready...');
  
  for (let i = 0; i < maxRetries; i++) {
    try {
      const response = await axios.get(`${STRAPI_URL}/admin`, {
        timeout: 5000
      });
      if (response.status === 200) {
        console.log('✅ Strapi is ready!');
        return true;
      }
    } catch (error) {
      console.log(`⏳ Attempt ${i + 1}/${maxRetries} - Strapi not ready yet...`);
      await new Promise(resolve => setTimeout(resolve, interval));
    }
  }
  
  throw new Error('❌ Strapi failed to start within timeout period');
}

async function createAdminUser() {
  console.log('👤 Creating admin user...');
  
  try {
    const response = await axios.post(
      `${STRAPI_URL}/admin/register-admin`,
      ADMIN_USER,
      {
        headers: {
          'Content-Type': 'application/json'
        },
        timeout: 10000
      }
    );
    
    const token = response.data?.data?.token || response.data?.token;
    
    if (!token) {
      throw new Error('No JWT token received from admin registration');
    }
    
    console.log('✅ Admin user created successfully');
    console.log(`🔑 JWT Token: ${token.substring(0, 20)}...`);
    
    return token;
  } catch (error) {
    if (error.response?.status === 400) {
      console.log('ℹ️ Admin user may already exist, trying to get existing token...');
      // 如果管理員已存在，嘗試登入
      return await loginAdminUser();
    }
    
    console.error('❌ Failed to create admin user:', error.response?.data || error.message);
    throw error;
  }
}

async function loginAdminUser() {
  console.log('🔑 Logging in as existing admin user...');
  
  try {
    const response = await axios.post(
      `${STRAPI_URL}/admin/login`,
      {
        email: ADMIN_USER.email,
        password: ADMIN_USER.password
      },
      {
        headers: {
          'Content-Type': 'application/json'
        },
        timeout: 10000
      }
    );
    
    const token = response.data?.data?.token || response.data?.token;
    
    if (!token) {
      throw new Error('No JWT token received from admin login');
    }
    
    console.log('✅ Admin login successful');
    return token;
  } catch (error) {
    console.error('❌ Failed to login admin user:', error.response?.data || error.message);
    throw error;
  }
}

async function createApiToken(jwtToken) {
  console.log('🎫 Creating API token...');
  
  try {
    const response = await axios.post(
      `${STRAPI_URL}/admin/api-tokens`,
      API_TOKEN_CONFIG,
      {
        headers: {
          'Content-Type': 'application/json',
          'Authorization': `Bearer ${jwtToken}`
        },
        timeout: 10000
      }
    );
    
    const apiToken = response.data?.data?.accessKey || response.data?.accessKey;
    
    if (!apiToken) {
      throw new Error('No API token received from token creation');
    }
    
    console.log('✅ API token created successfully');
    console.log(`🔑 API Token: ${apiToken.substring(0, 20)}...`);
    
    return apiToken;
  } catch (error) {
    console.error('❌ Failed to create API token:', error.response?.data || error.message);
    throw error;
  }
}

async function createTestConfig(apiToken) {
  console.log('📝 Creating test configuration...');
  
  const testConfig = {
    StrapiOptions: {
      StrapiUrl: STRAPI_URL,
      StrapiToken: apiToken
    }
  };
  
  const configPath = path.join(process.cwd(), '../../Further.Strapi.Tests/appsettings.Test.json');
  const configDir = path.dirname(configPath);
  
  try {
    // 確保目錄存在
    await fs.mkdir(configDir, { recursive: true });
    
    // 寫入配置檔案
    await fs.writeFile(configPath, JSON.stringify(testConfig, null, 2));
    
    console.log('✅ Test configuration created successfully');
    console.log(`📁 Config saved to: ${configPath}`);
    
    return configPath;
  } catch (error) {
    console.error('❌ Failed to create test configuration:', error.message);
    throw error;
  }
}

async function verifyApiAccess(apiToken) {
  console.log('🔍 Verifying API access...');
  
  try {
    const response = await axios.get(`${STRAPI_URL}/api/articles`, {
      headers: {
        'Authorization': `Bearer ${apiToken}`
      },
      timeout: 5000
    });
    
    console.log('✅ API access verified successfully');
    console.log(`📊 API Response: ${response.status} - ${JSON.stringify(response.data).substring(0, 100)}...`);
    
    return true;
  } catch (error) {
    console.error('❌ API access verification failed:', error.response?.data || error.message);
    throw error;
  }
}

async function main() {
  try {
    console.log('🚀 Starting Strapi CI setup...');
    
    // 1. 等待 Strapi 啟動
    await waitForStrapi();
    
    // 2. 建立管理員帳號
    const jwtToken = await createAdminUser();
    
    // 3. 建立 API Token
    const apiToken = await createApiToken(jwtToken);
    
    // 4. 建立測試配置
    await createTestConfig(apiToken);
    
    // 5. 驗證 API 存取
    await verifyApiAccess(apiToken);
    
    console.log('🎉 Strapi CI setup completed successfully!');
    console.log('📋 Summary:');
    console.log(`   - Admin User: ${ADMIN_USER.email}`);
    console.log(`   - API Token: ${apiToken.substring(0, 20)}...`);
    console.log(`   - Strapi URL: ${STRAPI_URL}`);
    
    process.exit(0);
  } catch (error) {
    console.error('💥 Setup failed:', error.message);
    process.exit(1);
  }
}

// 只在直接執行時運行
if (require.main === module) {
  main();
}

module.exports = {
  waitForStrapi,
  createAdminUser,
  createApiToken,
  createTestConfig,
  verifyApiAccess
};
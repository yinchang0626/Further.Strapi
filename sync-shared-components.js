/**
 * 確保所有 Strapi 專案都有一致的共用組件
 * 這個腳本可以在 CI/CD 或開發環境中執行，確保組件同步
 */

const fs = require('fs').promises;
const path = require('path');

// 共用組件定義 - 這是真實來源（Single Source of Truth）
const SHARED_COMPONENTS = {
  'shared/string-item': {
    collectionName: 'components_shared_string_items',
    info: {
      displayName: 'StringItem'
    },
    options: {},
    attributes: {
      value: {
        type: 'string'
      }
    },
    config: {}
  },
  'shared/media': {
    collectionName: 'components_shared_media',
    info: {
      displayName: 'Media',
      icon: 'file-video'
    },
    attributes: {
      file: {
        type: 'media',
        allowedTypes: ['images', 'files', 'videos']
      }
    }
  },
  'shared/slider': {
    collectionName: 'components_shared_sliders',
    info: {
      description: '',
      displayName: 'Slider',
      icon: 'address-book'
    },
    attributes: {
      files: {
        type: 'media',
        multiple: true,
        allowedTypes: ['images']
      }
    }
  }
};

// 需要同步的 Strapi 專案路徑
const STRAPI_PROJECTS = [
  './etc/strapi-integration-test',
  '../Tourmap.Booking.Strapi/etc/strapi-integration-test'
];

async function ensureComponentInProject(projectPath, componentPath, componentSchema) {
  const [category, name] = componentPath.split('/');
  const componentDir = path.join(projectPath, 'src', 'components', category);
  const componentFile = path.join(componentDir, `${name}.json`);
  
  try {
    // 檢查組件檔案是否存在
    const existingContent = await fs.readFile(componentFile, 'utf-8');
    const existingSchema = JSON.parse(existingContent);
    
    // 比較現有檔案與預期架構是否一致
    const expectedContent = JSON.stringify(componentSchema, null, 2);
    const currentContent = JSON.stringify(existingSchema, null, 2);
    
    if (currentContent === expectedContent) {
      console.log(`✅ ${projectPath}: Component ${componentPath} is up to date`);
      return false; // 沒有變更
    } else {
      console.log(`🔄 ${projectPath}: Component ${componentPath} needs update`);
      await fs.writeFile(componentFile, expectedContent);
      console.log(`✅ ${projectPath}: Component ${componentPath} updated`);
      return true; // 有變更
    }
  } catch (error) {
    if (error.code === 'ENOENT') {
      // 檔案不存在，建立組件
      console.log(`📝 ${projectPath}: Creating component ${componentPath}`);
      
      try {
        // 確保目錄存在
        await fs.mkdir(componentDir, { recursive: true });
        
        // 寫入組件檔案
        await fs.writeFile(componentFile, JSON.stringify(componentSchema, null, 2));
        
        console.log(`✅ ${projectPath}: Component ${componentPath} created`);
        return true; // 有變更
      } catch (createError) {
        console.error(`❌ ${projectPath}: Failed to create component ${componentPath}:`, createError.message);
        throw createError;
      }
    } else {
      console.error(`❌ ${projectPath}: Error processing component ${componentPath}:`, error.message);
      throw error;
    }
  }
}

async function syncSharedComponents() {
  console.log('🔄 Syncing shared components across all Strapi projects...');
  
  let totalChanges = 0;
  
  for (const projectPath of STRAPI_PROJECTS) {
    console.log(`\n📁 Processing project: ${projectPath}`);
    
    // 檢查專案目錄是否存在
    try {
      await fs.access(projectPath);
    } catch (error) {
      console.log(`⚠️ Project directory not found: ${projectPath}, skipping...`);
      continue;
    }
    
    let projectChanges = 0;
    
    for (const [componentPath, componentSchema] of Object.entries(SHARED_COMPONENTS)) {
      try {
        const hasChange = await ensureComponentInProject(projectPath, componentPath, componentSchema);
        if (hasChange) {
          projectChanges++;
        }
      } catch (error) {
        console.error(`❌ Failed to process component ${componentPath} in ${projectPath}:`, error.message);
        // 繼續處理其他組件
      }
    }
    
    console.log(`📊 ${projectPath}: ${projectChanges} components updated`);
    totalChanges += projectChanges;
  }
  
  console.log(`\n🎉 Sync completed! Total changes: ${totalChanges}`);
  
  if (totalChanges > 0) {
    console.log('\n💡 Next steps:');
    console.log('   1. Restart any running Strapi instances to regenerate TypeScript definitions');
    console.log('   2. Run `npm run develop` or `npx strapi ts:generate-types` to update .d.ts files');
  }
  
  return totalChanges;
}

async function listComponents() {
  console.log('📋 Available shared components:');
  
  for (const [componentPath, componentSchema] of Object.entries(SHARED_COMPONENTS)) {
    console.log(`\n🧩 ${componentPath}`);
    console.log(`   Collection: ${componentSchema.collectionName}`);
    console.log(`   Display Name: ${componentSchema.info.displayName}`);
    console.log(`   Attributes: ${Object.keys(componentSchema.attributes).join(', ')}`);
  }
}

async function validateProjects() {
  console.log('🔍 Validating project structures...');
  
  for (const projectPath of STRAPI_PROJECTS) {
    console.log(`\n📁 Checking: ${projectPath}`);
    
    try {
      await fs.access(projectPath);
      console.log(`✅ Project exists`);
      
      const componentsDir = path.join(projectPath, 'src', 'components');
      await fs.access(componentsDir);
      console.log(`✅ Components directory exists`);
      
      const sharedDir = path.join(componentsDir, 'shared');
      await fs.access(sharedDir);
      console.log(`✅ Shared components directory exists`);
      
    } catch (error) {
      console.log(`❌ Project structure issue: ${error.message}`);
    }
  }
}

async function main() {
  const command = process.argv[2];
  
  switch (command) {
    case 'sync':
      await syncSharedComponents();
      break;
    case 'list':
      await listComponents();
      break;
    case 'validate':
      await validateProjects();
      break;
    default:
      console.log('🛠️ Shared Components Management Tool');
      console.log('\nUsage:');
      console.log('  node sync-shared-components.js sync      - Sync components across all projects');
      console.log('  node sync-shared-components.js list      - List available shared components');
      console.log('  node sync-shared-components.js validate  - Validate project structures');
      console.log('\nExample:');
      console.log('  node sync-shared-components.js sync');
      break;
  }
}

// 只在直接執行時運行
if (require.main === module) {
  main().catch(error => {
    console.error('💥 Error:', error.message);
    process.exit(1);
  });
}

module.exports = {
  SHARED_COMPONENTS,
  syncSharedComponents,
  ensureComponentInProject,
  listComponents,
  validateProjects
};
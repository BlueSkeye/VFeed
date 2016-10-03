namespace SSUpdate
{
    internal static class SqlServerSchema
    {
        internal const string CreateSqlServerSchema = 
            "CREATE TABLE[nvd_db] (" +
            "        [cveid] sysname NOT NULL PRIMARY KEY CLUSTERED," +
            "        [date_published] date NULL," +
            "        [date_modified] date NULL," +
            "        [summary] text NULL," +
            "        [cvss_base] text NULL," +
            "        [cvss_impact] text NULL," + 
            "        [cvss_exploit] text NULL," + 
            "        [cvss_access_vector] text NULL," +
            "        [cvss_access_complexity] text NULL," +
            "        [cvss_authentication] text NULL," +
            "        [cvss_confidentiality_impact] text NULL," +
            "        [cvss_integrity_impact] text NULL," +
            "        [cvss_availability_impact] text NULL," +
            "        [cvss_vector] text NULL);" +
            "CREATE TABLE[cwe_db] (" +
            "        [cweid] sysname NOT NULL PRIMARY KEY CLUSTERED," +
            "        [cwetitle] sysname NULL);" +
            "CREATE TABLE[cve_cwe] (" +
            "        [cweid] sysname NULL CONSTRAINT CveCweCwe FOREIGN KEY REFERENCES [cwe_db]([cweid])," +
            "        [cveid] sysname NOT NULL CONSTRAINT CveCweNvd FOREIGN KEY REFERENCES [nvd_db]([cveid]));" +
            "CREATE TABLE[cwe_category] (" +
            "        [categoryid] sysname NULL," +
            "        [categorytitle] sysname NULL," +
            "        [cweid] sysname NOT NULL CONSTRAINT CweCategoryCwe FOREIGN KEY REFERENCES [cwe_db]([cweid]));" +
            "CREATE TABLE[cwe_capec] (" +
            "        [capecid] sysname NULL," +
            "        [cweid] sysname NOT NULL CONSTRAINT CweCapecCwe FOREIGN KEY REFERENCES [cwe_db]([cweid]));" +
            "CREATE TABLE[cve_cpe] (" +
            "        [cpeid] nvarchar(512) NOT NULL," +
            "        [cveid] sysname NOT NULL CONSTRAINT CveCpeNvd FOREIGN KEY REFERENCES [nvd_db]([cveid])" +
            "        CONSTRAINT [PK_cve_cpe] PRIMARY KEY CLUSTERED ([cpeid], [cveid]));" +
            "CREATE TABLE[cve_reference] (" +
            "        [refsource] sysname NULL," +
            "        [refname] nvarchar(512) NULL," +
            "        [cveid] sysname NOT NULL CONSTRAINT CveReferenceNvd FOREIGN KEY REFERENCES [nvd_db]([cveid]));" +
            "CREATE TABLE[map_cve_aixapar] (" +
            "        [aixaparid] sysname NULL," +
            "        [cveid] sysname NOT NULL CONSTRAINT CveAixaparNvd FOREIGN KEY REFERENCES [nvd_db]([cveid]));" +
            "CREATE TABLE[map_cve_redhat] (" +
            "        [redhatid] sysname NULL," +
            "        [redhatovalid] sysname NULL," +
            "        [redhatupdatedesc] sysname NULL," +
            "        [cveid] sysname NOT NULL CONSTRAINT CveRedhatNvd FOREIGN KEY REFERENCES [nvd_db]([cveid]));" +
            "CREATE TABLE[map_redhat_bugzilla] (" +
            "        [advisory_dateissue] sysname NULL," +
            "        [bugzillaid] sysname NULL," +
            "        [bugzillatitle] sysname NULL," +
            "        [redhatid] text NOT NULL);" +
            "CREATE TABLE[map_cve_suse] (" +
            "        [suseid] sysname NULL," +
            "        [cveid] sysname NOT NULL CONSTRAINT CveSuseNvd FOREIGN KEY REFERENCES [nvd_db]([cveid]));" +
            "CREATE TABLE[map_cve_debian] (" +
            "        [debianid] sysname NULL," +
            "        [cveid] sysname NOT NULL CONSTRAINT CveDebianNvd FOREIGN KEY REFERENCES [nvd_db]([cveid]));" +
            "CREATE TABLE[map_cve_mandriva] (" +
            "        [mandrivaid] sysname NULL," +
            "        [cveid] sysname NOT NULL CONSTRAINT CveMandrivaNvd FOREIGN KEY REFERENCES [nvd_db]([cveid]));" +
            "CREATE TABLE[map_cve_saint] (" +
            "        [saintexploitid] sysname NULL," +
            "        [saintexploittitle] sysname NULL," +
            "        [saintexploitlink] sysname NULL," +
            "        [cveid] sysname NOT NULL CONSTRAINT CveSaintNvd FOREIGN KEY REFERENCES [nvd_db]([cveid]));" +
            "CREATE TABLE[map_cve_milw0rm] (" +
            "        [milw0rmid] sysname NULL," +
            "        [cveid] sysname NOT NULL CONSTRAINT CveMilw0rmNvd FOREIGN KEY REFERENCES [nvd_db]([cveid]));" +
            "CREATE TABLE[map_cve_osvdb] (" +
            "        [osvdbid] sysname NULL," +
            "        [cveid] sysname NOT NULL CONSTRAINT CveOsvdbNvd FOREIGN KEY REFERENCES [nvd_db]([cveid]));" +
            "CREATE TABLE[map_cve_nessus] (" +
            "        [nessus_script_id] sysname NULL," +
            "        [nessus_script_file] sysname NULL," +
            "        [nessus_script_name] sysname NULL," +
            "        [nessus_script_family] sysname NULL," +
            "        [cveid] sysname NOT NULL CONSTRAINT CveNessusNvd FOREIGN KEY REFERENCES [nvd_db]([cveid]));" +
            "CREATE TABLE[map_cve_msf] (" +
            "        [msfid] sysname NULL," +
            "        [msf_script_file] sysname NULL," +
            "        [msf_script_name] sysname NULL," +
            "        [cveid] sysname NOT NULL CONSTRAINT CveMsfNvd FOREIGN KEY REFERENCES [nvd_db]([cveid]));" +
            "CREATE TABLE[map_cve_openvas] (" +
            "        [openvas_script_id] sysname NULL," +
            "        [openvas_script_file] sysname NULL," +
            "        [openvas_script_name] sysname NULL," +
            "        [openvas_script_family] sysname NULL," +
            "        [cveid] sysname NOT NULL CONSTRAINT CveOpenvasNvd FOREIGN KEY REFERENCES [nvd_db]([cveid]));" +
            "CREATE TABLE[map_cve_scip] (" +
            "        [scipid] sysname NULL," +
            "        [sciplink] sysname NULL," +
            "        [cveid] sysname NOT NULL CONSTRAINT CveScipNvd FOREIGN KEY REFERENCES [nvd_db]([cveid]));" +
            "CREATE TABLE[map_cve_iavm] (" +
            "        [iavmid] sysname NULL," +
            "        [disakey] sysname NULL," +
            "        [iavmtitle] sysname NULL," +
            "        [cveid] sysname NOT NULL CONSTRAINT CveIavmNvd FOREIGN KEY REFERENCES [nvd_db]([cveid]));" +
            "CREATE TABLE[map_cve_cisco] (" +
            "        [ciscoid] sysname NULL," +
            "        [cveid] sysname NOT NULL CONSTRAINT CveCiscoNvd FOREIGN KEY REFERENCES [nvd_db]([cveid]));" +
            "CREATE TABLE[map_cve_ubuntu] (" +
            "        [ubuntuid] sysname NULL," +
            "        [cveid] sysname NOT NULL CONSTRAINT CveUbuntuNvd FOREIGN KEY REFERENCES [nvd_db]([cveid]));" +
            "CREATE TABLE[map_cve_gentoo] (" +
            "        [gentooid] sysname NULL," +
            "        [cveid] sysname NOT NULL CONSTRAINT CveGentooNvd FOREIGN KEY REFERENCES [nvd_db]([cveid]));" +
            "CREATE TABLE[map_cve_fedora] (" +
            "        [fedoraid] sysname NULL," +
            "        [cveid] sysname NOT NULL CONSTRAINT CveFedoraNvd FOREIGN KEY REFERENCES [nvd_db]([cveid]));" +
            "CREATE TABLE[map_cve_certvn] (" +
            "        [certvuid] sysname NULL," +
            "        [certvulink] sysname NULL," +
            "        [cveid] sysname NOT NULL CONSTRAINT CveCertvnNvd FOREIGN KEY REFERENCES [nvd_db]([cveid]));" +
            "CREATE TABLE[map_cve_ms] (" +
            "        [msid] sysname NULL," +
            "        [mstitle] sysname NULL," +
            "        [cveid] sysname NOT NULL CONSTRAINT CveMsNvd FOREIGN KEY REFERENCES [nvd_db]([cveid]));" +
            "CREATE TABLE[map_cve_mskb] (" +
            "        [mskbid] sysname NULL," +
            "        [mskbtitle] sysname NULL," +
            "        [cveid] text NOT NULL);" +
            "CREATE TABLE[map_cve_snort] (" +
            "        [snort_id] sysname NULL," +
            "        [snort_sig] sysname NULL," +
            "        [snort_classtype] sysname NULL," +
            "        [cveid] sysname NOT NULL CONSTRAINT CveSnortNvd FOREIGN KEY REFERENCES [nvd_db]([cveid]));" +
            "CREATE TABLE[map_cve_suricata] (" +
            "        [suricata_id] sysname NULL," +
            "        [suricata_sig] sysname NULL," +
            "        [suricata_classtype] sysname NULL," +
            "        [cveid] sysname NOT NULL CONSTRAINT CveSuricataNvd FOREIGN KEY REFERENCES [nvd_db]([cveid]));" +
            "CREATE TABLE[map_cve_vmware] (" +
            "        [vmwareid] sysname NULL," +
            "        [cveid] sysname NOT NULL CONSTRAINT CveVmwareNvd FOREIGN KEY REFERENCES [nvd_db]([cveid]));" +
            "CREATE TABLE[map_cve_bid] (" +
            "        [bidid] sysname NULL," +
            "        [cveid] sysname NOT NULL CONSTRAINT CveBidNvd FOREIGN KEY REFERENCES [nvd_db]([cveid]));" +
            "CREATE TABLE[map_cve_hp] (" +
            "        [hpid] sysname NULL," +
            "        [hplink] sysname NULL," +
            "        [cveid] sysname NOT NULL CONSTRAINT CveHpNvd FOREIGN KEY REFERENCES [nvd_db]([cveid]));" +
            "CREATE TABLE[stat_new_cve] (" +
            "        [new_cve_id] sysname NULL CONSTRAINT CveNewNvd FOREIGN KEY REFERENCES [nvd_db]([cveid])," +
            "        [new_cve_summary] sysname NULL);" +
            "CREATE TABLE[map_cve_exploitdb] (" +
            "        [exploitdbid] sysname NULL," +
            "        [exploitdbscript] sysname NULL," +
            "        [cveid] sysname NOT NULL CONSTRAINT CveExploitdbNvd FOREIGN KEY REFERENCES [nvd_db]([cveid]));" +
            "CREATE TABLE[map_cve_nmap] (" +
            "        [nmap_script_id] sysname NULL," +
            "        [nmap_script_cat] sysname NULL," +
            "        [cveid] sysname NOT NULL CONSTRAINT CveNmapNvd FOREIGN KEY REFERENCES [nvd_db]([cveid]));" +
            "CREATE TABLE[map_cve_oval] (" +
            "        [ovalid] sysname NULL," +
            "        [ovalclass] sysname NULL," +
            "        [ovaltitle] sysname NULL," +
            "        [cpeid] sysname NULL," +
            "        [cveid] sysname NOT NULL CONSTRAINT CveOvalNvd FOREIGN KEY REFERENCES [nvd_db]([cveid]));" +
            "CREATE TABLE[map_cve_d2] (" +
            "        [d2_script_name] sysname NULL," +
            "        [d2_script_file] sysname NULL," +
            "        [cveid] sysname NOT NULL CONSTRAINT CveD2Nvd FOREIGN KEY REFERENCES [nvd_db]([cveid]));" +
            "CREATE TABLE[stat_vfeed_kpi] (" +
            "        [db_version] sysname NULL," +
            "        [total_cve] sysname NULL," +
            "        [total_cpe] sysname NULL," +
            "        [total_cwe] sysname NULL," +
            "        [total_capec] sysname NULL," +
            "        [total_bid] sysname NULL," +
            "        [total_osvdb] sysname NULL," +
            "        [total_certvu] sysname NULL," +
            "        [total_iavm] sysname NULL," +
            "        [total_scip] sysname NULL," +
            "        [total_aixapar] sysname NULL," +
            "        [total_suse] sysname NULL," +
            "        [total_ubuntu] sysname NULL," +
            "        [total_vmware] sysname NULL," +
            "        [total_cisco] sysname NULL," +
            "        [total_debian] sysname NULL," +
            "        [total_fedora] sysname NULL," +
            "        [total_gentoo] sysname NULL," +
            "        [total_hp] sysname NULL," +
            "        [total_mandriva] sysname NULL," +
            "        [total_ms] sysname NULL," +
            "        [total_mskb] sysname NULL," +
            "        [total_redhat] sysname NULL," +
            "        [total_redhat_bugzilla] sysname NULL," +
            "        [total_exploitdb] sysname NULL," +
            "        [total_msf] sysname NULL," +
            "        [total_milw0rm] sysname NULL," +
            "        [total_saint] sysname NULL," +
            "        [total_nessus] sysname NULL," +
            "        [total_openvas] sysname NULL," +
            "        [total_oval] sysname NULL," +
            "        [total_snort] sysname NULL," +
            "        [total_nmap] sysname NULL," +
            "        [total_suricata] sysname NULL," +
            "        [total_d2exploit] sysname NULL);" +
            "CREATE TABLE[capec_db] (" +
            "        [capecid] sysname NULL," +
            "        [capectitle] sysname NULL," +
            "        [attack] sysname NULL);" +
            "CREATE TABLE[capec_mit] (" +
            "        [mitigation] sysname NULL," +
            "        [capecid] text NOT NULL);" +
            "CREATE TABLE[cwe_wasc] (" +
            "        [wascname] sysname NULL," +
            "        [wascid] sysname NULL," +
            "        [cweid] sysname NOT NULL CONSTRAINT CveWascNvd FOREIGN KEY REFERENCES [nvd_db]([cveid]));";
    }
}

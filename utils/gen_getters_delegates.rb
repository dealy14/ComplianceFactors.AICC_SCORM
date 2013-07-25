def camel_case(value)
	return value if value !~ /_/ && value =~ /[A-Z]+.*/
	value.split('_').map{|e| e.capitalize}.join
end

counter = 1
file = File.new("js_fields.txt", "r")
getters = File.new('getters.txt', 'w')
delegates = File.new('delegates.txt', 'w')
delegate_maps = File.new('delegate_maps.txt', 'w')

types = {
	'CMIString256' => 'string',
	'CMIString4096' => 'string',
	'CMITime' => 'time',
	'CMITimespan' => 'time',
	'CMIInteger' => 'int?',
	'CMISInteger' => 'int?',
	'CMIDecimal' => 'double?',
	'CMIIdentifier' => 'string',
	'CMIFeedback' => 'string',
	'CMIIndex' => 'string',
	'CMIStatus2' => 'string'
}

while (line = file.gets())
	# puts(line)
	next if (/_(count|children)/.match(line))

	the_match = /\s*'([a-z_\.]*)'.*/.match(line)
	key = the_match[1]
	the_match = /'format': (CMI[a-zA-Z0-9]*)/.match(line)
	type = nil
	type = the_match[1] unless (the_match == nil)
	type = (types.has_key?(type)) ? types[type] : 'string'
	# puts "type = #{type}"

	tokens = key.split(".")
	basename = tokens[1..-1].map{|t| camel_case(t)}.join

	case type
	when 'string'
		# puts 'when string'
		getter = <<-EOG
			private string Get#{basename}(Dictionary<string, string> json_values)
			{
				string val;
				json_values.TryGetValue("#{key}", out val);

				return val;
			}

		EOG

		delegate = <<-EOS
	    private void #{basename}Delegate(string key, Dictionary<string, string> json_values)
        {
            string tmp = Get#{basename}(json_values);
            if (tmp != null)
                this.m_values[key] = tmp;
        }

		EOS
	when 'int?'
		# puts 'when int?'
		getter = <<-EOG
			private int? Get#{basename}(Dictionary<string, string> json_values)
			{
				int? ret = null;
				string tmp = null;
				json_values.TryGetValue("#{key}", out tmp);

				int i;
				bool res = Int32.TryParse(tmp, out i);
				if (res)
					ret = i;

				return ret;
			}

		EOG

		delegate = <<-EOS
	    private void #{basename}Delegate(string key, Dictionary<string, string> json_values)
        {
            int? tmp = Get#{basename}(json_values);
            if (tmp != null)
                this.m_values[key] = tmp;
        }

		EOS
	when 'double?'
		# puts 'when double?'
		getter = <<-EOG
			private double? Get#{basename}(Dictionary<string, string> json_values)
			{
				return GetDouble("#{key}", json_values);
			}

		EOG

		delegate = <<-EOS
	    private void #{basename}Delegate(string key, Dictionary<string, string> json_values)
        {
            double? tmp = Get#{basename}(json_values);
            if (tmp != null)
                this.m_values[key] = tmp;
        }

		EOS
	when 'time'
		# puts 'when time'
		getter = <<-EOG
			private int? Get#{basename}(Dictionary<string, string> json_values)
			{
				return GetTime("#{key}", json_values);
			}

		EOG

		delegate = <<-EOS
	    private void #{basename}Delegate(string key, Dictionary<string, string> json_values)
        {
            int? tmp = Get#{basename}(json_values);
            if (tmp != null)
                this.m_values[key] = tmp;
        }

		EOS
	end
	
	getters.write(getter)
	delegates.write(delegate)
	delegate_maps.write("m_delegates[\"#{key}\"] = #{basename}Delegate;\n")
end
file.close()
getters.close()
delegates.close()
delegate_maps.close()